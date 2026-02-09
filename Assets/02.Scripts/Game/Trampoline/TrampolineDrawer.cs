using CodeJelly.BounceOrBreak.Core.Logging;
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

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// Player Input을 받아 Trampoline을 그리고 생성하는 클래스.
    /// </summary>
    public class TrampolineDrawer : MonoBehaviour
    {
        #region Private Variables
        [Header("드로잉 중 피드백 시각화를 위한 renderer")]
        [SerializeField] private LineRenderer _drawingLineRenderer;

        private Camera _mainCamera;
        private bool _isDrawing;
        private List<Vector2> _currentDrawPoints;

        private TrampolineConfig _config;
        private TrampolineFactory _factory;
        private TrampolineType _activeType = TrampolineType.Straight;

        private IObjectPool<ITrampoline> _trampolinePool;
        private CompositeDisposable _disposables;
        #endregion


        #region Unity Lifecycle
        private void OnDestroy()
        {
            _factory?.ReleaseAll();
            _trampolinePool?.Clear();
            _disposables?.Dispose();
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// 클래스를 사용 가능한 상태로 초기화합니다.
        /// </summary>
        public async UniTask InitializeAsync(IProgress<float> progress, CancellationToken ct)
        {
            _mainCamera = Camera.main;
            _isDrawing = false;
            _currentDrawPoints = new List<Vector2>();

            // Config 로딩
            _config = await ResourceManager.Instance.LoadAsync<TrampolineConfig>(GameConstants.Trampoline.Keys.DEFAULT_CONFIG, ct);
            progress?.Report(0.2f);

            if (_config == null)
            {
                AppLogger.Error("[TrampolineDrawer] Failed to load TrampolineConfig.");

                return;
            }

            // Factory 초기화
            _factory = new TrampolineFactory();
            await _factory.InitializeAsync(ct);
            progress?.Report(0.5f);

            if (_factory == null)
            {
                AppLogger.Error("[TrampolineDrawer] Failed to initialize Factory.");

                return;
            }

            // Pool 초기화
            _disposables = new CompositeDisposable();

            InitializePool();
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
            _trampolinePool.Release(trampoline);
        }

        /// <summary>
        /// 사용할 Trampoline 타입을 변경합니다.
        /// </summary>
        public void SetTrampolineType(TrampolineType type)
        {
            AppLogger.Info($"[TrampolineDrawer] Trampoline type changed: {_activeType} -> {type}.");

            _activeType = type;
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// ObjectPool을 초기화합니다.
        /// </summary>
        private void InitializePool()
        {
            _trampolinePool = PoolExtensions.CreatePoolablePool
            (
                createFunc: CreateTrampolineInstance,
                defaultCapacity: GameConstants.Trampoline.POOL_DEFAULT_CAPACITY,
                maxSize: GameConstants.Trampoline.POOL_MAX_SIZE
            );

            AppLogger.Info("[TrampolineDrawer] ObjectPool initialized.");
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
            _drawingLineRenderer.startWidth = _config.LineWidth;
            _drawingLineRenderer.endWidth = _config.LineWidth;
            _drawingLineRenderer.material = _config.LineMaterial;
            _drawingLineRenderer.startColor = _config.LineColor;
            _drawingLineRenderer.endColor = _config.LineColor;
        }

        /// <summary>
        /// InputManager의 이벤트를 구독합니다.
        /// </summary>
        private void SubscribeInputEvents()
        {
            var inputManager = InputManager.Instance;

            inputManager.OnPlayerTouchStarted
                .Subscribe(position => StartDrawing(position))
                .AddTo(_disposables);

            inputManager.OnPlayerTouchCanceled
                .Subscribe(_ => EndDrawing())
                .AddTo(_disposables);

            inputManager.OnPlayerDragPerformed
                .Subscribe(position => UpdateDrawing(position))
                .AddTo(_disposables);

            AppLogger.Info("[TrampolineDrawer] Input events subscribed.");
        }

        /// <summary>
        /// Trampoline 인스턴스를 생성합니다.
        /// </summary>
        private ITrampoline CreateTrampolineInstance()
        {
            return _factory.Create(_activeType);
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

            // Point의 최소치 범위 확인
            if (_currentDrawPoints.Count < GameConstants.Trampoline.MIN_DRAW_POINTS)
            {
                AppLogger.Debug("[TrampolineDrawer] Not enough points to create Trampoline.");
                ClearDrawing();

                return;
            }

            // Trampoline 생성
            Vector2[] finalPoints = ProcessDrawPoints();
            SpawnTrampoline(finalPoints);
            ClearDrawing();

            AppLogger.Debug("[TrampolineDrawer] Drawing ended.");
        }

        /// <summary>
        /// DrawPoints를 Trampoline 타입에 맞게 가공합니다.
        /// </summary>
        private Vector2[] ProcessDrawPoints()
        {
            if (_activeType == TrampolineType.Straight)
            {
                Vector2 start = _currentDrawPoints[0];
                Vector2 end = _currentDrawPoints[_currentDrawPoints.Count - 1];

                return new Vector2[] { start, end };
            }
            else
            {
                return _currentDrawPoints.ToArray();
            }
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
        /// Trampoline 오브젝트를 Pool에서 가져와 생성합니다.
        /// </summary>
        private void SpawnTrampoline(Vector2[] points)
        {
            try
            {
                ITrampoline trampoline = _trampolinePool.Get();

                if (trampoline == null)
                {
                    AppLogger.Error("[TrampolineDrawer] Failed to get Trampoline from pool.");

                    return;
                }

                trampoline.Initialize(this, _config, points);

                AppLogger.Debug("[TrampolineDrawer] Trampoline spawned.");
            }
            catch (Exception ex)
            {
                AppLogger.Exception(ex);
            }
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
        #endregion
    }
}