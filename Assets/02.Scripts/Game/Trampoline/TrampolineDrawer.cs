using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Pause;
using CodeJelly.BounceOrBreak.Core.Pooling;
using CodeJelly.BounceOrBreak.Core.Resource;
using CodeJelly.BounceOrBreak.Game.Input;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// Player Input을 받아 Trampoline을 그리고 생성하는 클래스.
    /// </summary>
    public class TrampolineDrawer : MonoBehaviour, IPauseable
    {
        #region Injection
        private ResourceManager _resourceManager;
        private InputManager _inputManager;
        private Camera _mainCamera;

        [Inject]
        public void Construct(
            ResourceManager resourceManager,
            InputManager inputManager,
            Camera mainCamera)
        {
            _resourceManager = resourceManager;
            _inputManager = inputManager;
            _mainCamera = mainCamera;
        }
        #endregion


        #region Private Variables
        [Header("드로잉 중 피드백 시각화를 위한 renderer")]
        [SerializeField] private LineRenderer _drawingLineRenderer;

        private bool _isDrawing;
        private List<Vector2> _currentDrawPoints;
        private TrampolineType _activeType = TrampolineType.Straight;

        private TrampolineConfig _config;
        private TrampolineRegistry _registry;
        private TrampolineFactory _factory;

        /// <summary>
        /// TrampolineType별 ObjectPool.
        /// </summary>
        private readonly Dictionary<TrampolineType, IObjectPool<ITrampoline>> _pools = new Dictionary<TrampolineType, IObjectPool<ITrampoline>>();

        /// <summary>
        /// Pool에서 꺼내져 활성 상태인 Trampoline 목록.
        /// </summary>
        private readonly List<ITrampoline> _activeTrampolines = new List<ITrampoline>();

        /// <summary>
        /// 드로잉으로 생성된 Trampoline 목록. 그린 순서를 유지하며, 스테이지 Trampoline은 포함하지 않습니다.
        /// </summary>
        private readonly List<ITrampoline> _drawnTrampolines = new List<ITrampoline>();

        /// <summary>
        /// 생성된 Trampoline들이 배치될 루트.
        /// </summary>
        private Transform _root;

        private CompositeDisposable _disposables;
        #endregion


        #region Unity Lifecycle
        private void OnDestroy()
        {
            // Clear는 Pool에 있는 Trampoline만 파괴하므로, 먼저 회수해야 모두 Destroy를 거침
            DespawnAll();

            foreach (IObjectPool<ITrampoline> pool in _pools.Values)
            {
                pool.Clear();
            }

            _factory?.ReleaseAll();
            _disposables?.Dispose();

            if (_registry != null)
            {
                _resourceManager.Release(GameConstants.Trampoline.Keys.REGISTRY, ResourceReleaseContext.Cleanup);
            }

            if (_config != null)
            {
                _resourceManager.Release(GameConstants.Trampoline.Keys.DEFAULT_CONFIG, ResourceReleaseContext.Cleanup);
            }
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// 클래스를 사용 가능한 상태로 초기화합니다.
        /// </summary>
        public async UniTask InitializeAsync(IProgress<float> progress, CancellationToken ct)
        {
            _isDrawing = false;
            _currentDrawPoints = new List<Vector2>();

            // Config 로드
            _config = await _resourceManager.LoadAsync<TrampolineConfig>(GameConstants.Trampoline.Keys.DEFAULT_CONFIG, ct);
            progress?.Report(0.2f);

            if (_config == null)
            {
                AppLogger.Error("[TrampolineDrawer] Failed to load TrampolineConfig.");

                return;
            }

            // Registry 로드
            _registry = await _resourceManager.LoadAsync<TrampolineRegistry>(GameConstants.Trampoline.Keys.REGISTRY, ct);
            progress?.Report(0.35f);

            if (_registry == null)
            {
                AppLogger.Error("[TrampolineDrawer] Failed to load TrampolineRegistry.");

                return;
            }

            // 자식으로 두어 부모의 소속 씬을 따르므로, 초기화 시점의 활성 씬과 무관
            _root = new GameObject(GameConstants.Trampoline.ROOT_NAME).transform;
            _root.SetParent(transform, false);

            // Factory 초기화
            _factory = new TrampolineFactory(_resourceManager, _registry, _root);
            await _factory.InitializeAsync(ct);
            progress?.Report(0.5f);

            // Pool 초기화
            _disposables = new CompositeDisposable();

            InitializePools();
            progress?.Report(0.8f);

            // LineRenderer 초기화
            if (_drawingLineRenderer == null)
            {
                AppLogger.Error("[TrampolineDrawer] DrawingLineRenderer is not assigned.");

                return;
            }

            InitializeDrawingLineRenderer();
            SubscribeInputEvents();
            progress?.Report(1.0f);

            AppLogger.Info("[TrampolineDrawer] Initialized.");
        }

        /// <summary>
        /// Trampoline을 Pool에 반환합니다.
        /// </summary>
        /// <param name="trampoline">반환할 Trampoline</param>
        public void ReturnToPool(ITrampoline trampoline)
        {
            _activeTrampolines.Remove(trampoline);
            _drawnTrampolines.Remove(trampoline);

            GetPool(trampoline.Type).Release(trampoline);
        }

        /// <summary>
        /// 활성 상태인 모든 Trampoline을 Pool에 반환합니다.
        /// </summary>
        public void DespawnAll()
        {
            // ReturnToPool이 순회 중 목록을 수정하므로 역순으로 순회
            for (int i = _activeTrampolines.Count - 1; 0 <= i; i--)
            {
                ReturnToPool(_activeTrampolines[i]);
            }
        }

        /// <summary>
        /// TrampolineSpawnData에 정의된 Trampoline을 Pool에서 꺼내 스폰합니다.
        /// </summary>
        /// <param name="spawnData">스폰할 Trampoline의 정의</param>
        public void Spawn(TrampolineSpawnData spawnData)
        {
            Spawn(spawnData.Type, spawnData.Config, spawnData.Points);
        }

        /// <summary>
        /// 사용할 TrampolineType을 변경합니다.
        /// </summary>
        public void SetTrampolineType(TrampolineType type)
        {
            AppLogger.Info($"[TrampolineDrawer] Trampoline type changed: {_activeType} -> {type}.");

            _activeType = type;
        }

        /// <summary>
        /// 진행 중인 드로잉을 취소합니다.
        /// </summary>
        public void CancelDrawing()
        {
            _isDrawing = false;
            ClearDrawing();
        }

        /// <summary>
        /// IPauseable.Pause Implementation
        /// </summary>
        public void Pause()
        {
            CancelDrawing();

            foreach (ITrampoline trampoline in _activeTrampolines)
            {
                trampoline.Pause();
            }
        }

        /// <summary>
        /// IPauseable.Resume Implementation
        /// </summary>
        public void Resume()
        {
            foreach (ITrampoline trampoline in _activeTrampolines)
            {
                trampoline.Resume();
            }
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 선분을 드로잉 경계 안으로 잘라냅니다.
        /// 끝점을 경계와의 교점으로 옮기므로 기울기가 유지됩니다.
        /// </summary>
        private static Vector2[] ClipToBoundary(Vector2 start, Vector2 end)
        {
            float boundary = GameConstants.Trampoline.DRAW_BOUNDARY_X;

            // 양 끝이 같은 쪽 경계 밖이면 선분 전체가 플레이 영역 밖이라 남는 구간이 없음
            bool isOutside = (start.x < -boundary && end.x < -boundary)
                          || (boundary < start.x && boundary < end.x);

            if (isOutside == true)
            {
                return Array.Empty<Vector2>();
            }

            return new Vector2[] { ClipEndpoint(start, end, boundary), ClipEndpoint(end, start, boundary) };
        }

        /// <summary>
        /// 끝점이 경계 밖이면 선분과 경계의 교점으로 옮깁니다.
        /// ClipToBoundary에서만 호출됩니다.
        /// </summary>
        private static Vector2 ClipEndpoint(Vector2 point, Vector2 other, float boundary)
        {
            if (-boundary <= point.x && point.x <= boundary)
            {
                return point;
            }

            float clampedX = Mathf.Clamp(point.x, -boundary, boundary);
            float ratio = (clampedX - other.x) / (point.x - other.x);

            return Vector2.Lerp(other, point, ratio);
        }

        /// <summary>
        /// 가공된 DrawPoints가 Trampoline을 이룰 수 있는지 확인합니다.
        /// </summary>
        private static bool CanFormTrampoline(Vector2[] points)
        {
            if (points.Length < GameConstants.Trampoline.MIN_DRAW_POINTS)
            {
                return false;
            }

            float length = 0f;

            for (int i = 1; i < points.Length; i++)
            {
                length += Vector2.Distance(points[i - 1], points[i]);
            }

            return GameConstants.Trampoline.MIN_POINT_DISTANCE <= length;
        }

        /// <summary>
        /// Factory가 로드한 TrampolineType마다 ObjectPool을 초기화합니다.
        /// </summary>
        private void InitializePools()
        {
            foreach (TrampolineType type in _factory.LoadedTypes)
            {
                // Pool마다 자기 TrampolineType을 고정해야 반환되는 인스턴스의 TrampolineType이 보장됨
                _pools[type] = PoolExtensions.CreatePoolablePool
                (
                    createFunc: () => _factory.Create(type),
                    defaultCapacity: GameConstants.Trampoline.POOL_DEFAULT_CAPACITY,
                    maxSize: GameConstants.Trampoline.POOL_MAX_SIZE
                );
            }

            AppLogger.Info($"[TrampolineDrawer] ObjectPools initialized. ({_pools.Count} types)");
        }

        /// <summary>
        /// DrawingLineRenderer를 초기화합니다.
        /// </summary>
        private void InitializeDrawingLineRenderer()
        {
            if (_drawingLineRenderer == null || _config == null)
            {
                AppLogger.Error($"[TrampolineDrawer] Initializing DrawingLineRenderer failed. DrawingLineRenderer: {_drawingLineRenderer}, Config: {_config}.");

                return;
            }

            _drawingLineRenderer.positionCount = 0;
            // 미리보기는 확정 전 임시 선이라 LineRenderer를 유지하며, 재질과 색은 Drawer 프리팹의 LineRenderer에 설정된 값을 사용
            _drawingLineRenderer.startWidth = _config.Thickness;
            _drawingLineRenderer.endWidth = _config.Thickness;
        }

        /// <summary>
        /// InputManager의 이벤트를 구독합니다.
        /// </summary>
        private void SubscribeInputEvents()
        {
            _inputManager.OnPlayerTouchStarted
                .Subscribe(position => StartDrawing(position))
                .AddTo(_disposables);

            _inputManager.OnPlayerTouchCanceled
                .Subscribe(_ => EndDrawing())
                .AddTo(_disposables);

            _inputManager.OnPlayerDragPerformed
                .Subscribe(position => UpdateDrawing(position))
                .AddTo(_disposables);

            AppLogger.Info("[TrampolineDrawer] Input events subscribed.");
        }

        /// <summary>
        /// 지정된 형태와 설정의 Trampoline을 Pool에서 꺼내 스폰합니다.
        /// </summary>
        /// <param name="type">스폰할 Trampoline의 형태</param>
        /// <param name="config">적용할 Trampoline 설정</param>
        /// <param name="points">LineRenderer의 point 배열</param>
        /// <returns>스폰된 Trampoline. 실패 시 null</returns>
        private ITrampoline Spawn(TrampolineType type, TrampolineConfig config, Vector2[] points)
        {
            if (config == null)
            {
                AppLogger.Error($"[TrampolineDrawer] TrampolineConfig is null for {type}.");

                return null;
            }

            IObjectPool<ITrampoline> pool = GetPool(type);
            ITrampoline trampoline = pool.Get();

            if (trampoline.Initialize(this, config, points) == false)
            {
                AppLogger.Error("[TrampolineDrawer] Failed to initialize Trampoline. Returning to pool.");
                pool.Release(trampoline);

                return null;
            }

            _activeTrampolines.Add(trampoline);

            AppLogger.Debug($"[TrampolineDrawer] Trampoline spawned: {type}");

            return trampoline;
        }

        /// <summary>
        /// 드로잉으로 생성된 Trampoline을 목록에 등록하고, 최대 개수를 넘으면 가장 오래된 것을 회수합니다.
        /// </summary>
        private void RegisterDrawn(ITrampoline trampoline)
        {
            _drawnTrampolines.Add(trampoline);

            if (_drawnTrampolines.Count <= GameConstants.Trampoline.MAX_DRAWN_COUNT)
            {
                return;
            }

            // ReturnToPool이 목록에서도 제거하므로 별도 정리 불필요
            ReturnToPool(_drawnTrampolines[0]);
        }

        /// <summary>
        /// 지정된 TrampolineType의 ObjectPool을 반환합니다.
        /// </summary>
        private IObjectPool<ITrampoline> GetPool(TrampolineType type)
        {
            if (_pools.TryGetValue(type, out IObjectPool<ITrampoline> pool) == false)
            {
                throw new InvalidOperationException($"[TrampolineDrawer] No pool initialized for TrampolineType: {type}");
            }

            return pool;
        }

        /// <summary>
        /// 드로잉을 시작합니다.
        /// </summary>
        private void StartDrawing(Vector2 screenPosition)
        {
            _isDrawing = true;
            _currentDrawPoints.Clear();

            Vector2 worldPosition = GetWorldPosition(screenPosition);
            _currentDrawPoints.Add(worldPosition);

            _drawingLineRenderer.positionCount = 1;
            _drawingLineRenderer.SetPosition(0, worldPosition);

            AppLogger.Debug("[TrampolineDrawer] Drawing started.");
        }

        /// <summary>
        /// 드로잉 중 라인을 업데이트합니다.
        /// </summary>
        private void UpdateDrawing(Vector2 screenPosition)
        {
            if (_isDrawing == false)
            {
                return;
            }

            Vector2 worldPosition = GetWorldPosition(screenPosition);

            // _currentDrawPoints는 항상 1개 이상 있음이 보장됨. (StartDrawing에서 추가)
            Vector2 lastPoint = _currentDrawPoints[^1];

            // Point 간 최소 거리 확인
            if (Vector2.Distance(lastPoint, worldPosition) < GameConstants.Trampoline.MIN_POINT_DISTANCE)
            {
                return;
            }

            // Point의 최대치 범위 확인
            if (_currentDrawPoints.Count >= GameConstants.Trampoline.MAX_LINE_POINTS)
            {
                return;
            }

            _currentDrawPoints.Add(worldPosition);

            // LineRenderer 업데이트
            _drawingLineRenderer.positionCount = _currentDrawPoints.Count;
            _drawingLineRenderer.SetPosition(_currentDrawPoints.Count - 1, worldPosition);
        }

        /// <summary>
        /// 드로잉을 종료하고 Trampoline을 생성합니다.
        /// </summary>
        private void EndDrawing()
        {
            if (_isDrawing == false)
            {
                return;
            }

            _isDrawing = false;

            Vector2[] finalPoints = ProcessDrawPoints();

            if (CanFormTrampoline(finalPoints) == false)
            {
                AppLogger.Debug("[TrampolineDrawer] Drawing cannot form a Trampoline.");
                ClearDrawing();

                return;
            }

            // Trampoline 생성
            ITrampoline trampoline = Spawn(_activeType, _config, finalPoints);
            ClearDrawing();

            if (trampoline != null)
            {
                RegisterDrawn(trampoline);
            }

            AppLogger.Debug("[TrampolineDrawer] Drawing ended.");
        }

        /// <summary>
        /// 드로잉을 초기화합니다.
        /// </summary>
        private void ClearDrawing()
        {
            _drawingLineRenderer.positionCount = 0;
            _currentDrawPoints.Clear();
        }

        /// <summary>
        /// 스크린 좌표를 월드 좌표로 변환합니다.
        /// Camera의 nearClipPlane을 Z값으로 사용하여 2D 평면 상의 월드 좌표를 계산합니다.
        /// </summary>
        private Vector2 GetWorldPosition(Vector2 screenPosition)
        {
            Vector3 screenPosition3D = new Vector3
            (
                screenPosition.x,
                screenPosition.y,
                _mainCamera.nearClipPlane
            );

            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition3D);

            return new Vector2(worldPosition.x, worldPosition.y);
        }

        /// <summary>
        /// DrawPoints를 TrampolineType에 맞게 가공합니다.
        /// </summary>
        private Vector2[] ProcessDrawPoints()
        {
            if (_activeType == TrampolineType.Straight)
            {
                return ClipToBoundary(_currentDrawPoints[0], _currentDrawPoints[^1]);
            }
            else
            {
                // TODO: Freeform 경계 처리
                return _currentDrawPoints.ToArray();
            }
        }
        #endregion
    }
}