using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// 씬에 미리 배치되거나 런타임에 동적 생성되는 정적 일자형 Trampoline.
    /// TrampolineDrawer 없이 독립적으로 동작합니다.
    /// </summary>
    public class StaticTrampoline : TrampolineBase
    {
        #region Properties
        /// <summary>
        /// ITrampoline.Type Implementation
        /// </summary>
        public override TrampolineType Type => TrampolineType.Static;
        #endregion


        #region Private Variables
        [Header("Trampoline Settings")]
        [Tooltip("씬에 배치된 경우 Config를 직접 참조.")]
        [SerializeField] private TrampolineConfig _staticConfig; // 씬에 배치된 Trampoline의 경우 Initialize가 호출되지 않아 config를 주입받지 않으므로 직접 참조.

        [Tooltip("Trampoline 시작점")]
        [SerializeField] private Vector2 _startPoint;

        [Tooltip("Trampoline 끝점")]
        [SerializeField] private Vector2 _endPoint;

        private Vector2 _lineDirection;
        private Vector2 _normal;
        private bool _isPlacedOnScene;
        #endregion


        #region Unity Lifecycle
        private void Awake()
        {
            if (_staticConfig != null)
            {
                _isPlacedOnScene = true;
                InitializeInternal(_staticConfig);
                AppLogger.Debug("[StaticTrampoline] Initialized (Scene-Placed).");
            }
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// ITrampoline.Initialize Implementation
        /// 런타임에 동적 생성 시 사용됩니다.
        /// </summary>
        /// <returns>초기화 성공 시 true</returns>
        public override bool Initialize(TrampolineDrawer drawer, TrampolineConfig config, Vector2[] points)
        {
            if (points.Length != 2)
            {
                AppLogger.Error($"[StaticTrampoline] Invalid points count: {points.Length}. Expected 2.");

                return false;
            }

            _drawer = drawer;
            _startPoint = points[0];
            _endPoint = points[1];
            _isPlacedOnScene = false;

            InitializeInternal(config);
            AppLogger.Debug("[StaticTrampoline] Initialized (Runtime).");

            return true;
        }
        #endregion


        #region Protected Methods
        /// <summary>
        /// LineRenderer를 설정합니다.
        /// </summary>
        protected override void SetupLineRenderer()
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.startWidth = _config.LineWidth;
            _lineRenderer.endWidth = _config.LineWidth;
            _lineRenderer.material = _config.LineMaterial;
            _lineRenderer.startColor = _config.LineColor;
            _lineRenderer.endColor = _config.LineColor;

            _lineRenderer.SetPosition(0, _startPoint);
            _lineRenderer.SetPosition(1, _endPoint);
        }

        /// <summary>
        /// EdgeCollider2D를 설정합니다.
        /// </summary>
        protected override void SetupCollider()
        {
            _edgeCollider.points = new Vector2[] { _startPoint, _endPoint };
        }

        /// <summary>
        /// 라인의 특정 지점에서의 접촉 정보를 계산합니다.
        /// </summary>
        protected override ContactInfo AnalyzeContactInfo(Vector2 contactPoint)
        {
            float distanceFromStart = Vector2.Distance(_startPoint, contactPoint);

            return new ContactInfo
            {
                ContactPoint = contactPoint,
                Normal = _normal,
                DistanceFromStart = distanceFromStart,
                ClosestSegmentIndex = 0  // 선분이 하나뿐이므로 항상 0
            };
        }

        /// <summary>
        /// Scene-Placed와 Runtime에 따라 적합한 방식으로 Trampoline을 정리합니다.
        /// </summary>
        protected override void Despawn()
        {
            if (_isInitialized == false)
            {
                return;
            }

            AppLogger.Info($"[{Type}Trampoline] Despawned. UsageCount: {UsageCount}, Lifetime: {Lifetime:F2}s");

            _isInitialized = false;
            CancelLifetimeTimer();

            if (_isPlacedOnScene == true)
            {
                GameObject.Destroy(gameObject);
            }
            else
            {
                _drawer?.ReturnToPool(this);
            }
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 내부 초기화 로직을 수행합니다.
        /// Scene-Placed와 Runtime이 공통으로 사용합니다.
        /// </summary>
        private void InitializeInternal(TrampolineConfig config)
        {
            _config = config;

            InitializeComponents();
            CalculateLineProperties();
            SetupLineRenderer();
            SetupCollider();
            SetupPlatformEffector();

            Spawn();

            AppLogger.Debug("[StaticTrampoline] Initialized (Internal).");
        }

        /// <summary>
        /// 라인의 속성들을 미리 계산합니다.
        /// </summary>
        private void CalculateLineProperties()
        {
            _lineLength = Vector2.Distance(_startPoint, _endPoint);
            _lineDirection = (_endPoint - _startPoint).normalized;
            _normal = new Vector2(-_lineDirection.y, _lineDirection.x); // 90도 반시계방향 회전

            // 노말 벡터는 항상 Upward를 향해야 함
            if (_normal.y < 0f)
            {
                _normal = -_normal;
            }
        }
        #endregion


        #region Debug
#if UNITY_EDITOR
        /// <summary>
        /// 씬 뷰에서 Trampoline의 배치를 시각화합니다.
        /// 위치 조정은 Transform 이동이 아닌 startPoint/endPoint 값으로 해야 합니다.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (_startPoint == _endPoint)
            {
                return;
            }

            Color lineColor = (_staticConfig != null) ? _staticConfig.LineColor : Color.white;
            float endPointRadius = (_staticConfig != null) ? _staticConfig.LineWidth : GameConstants.Trampoline.DEFAULT_LINE_WIDTH;

            // 라인 본체와 양 끝점
            Gizmos.color = lineColor;
            Gizmos.DrawLine(_startPoint, _endPoint);
            Gizmos.DrawSphere(_startPoint, endPointRadius);
            Gizmos.DrawSphere(_endPoint, endPointRadius);

            if (_staticConfig == null)
            {
                return;
            }

            // 중앙 보너스 판정 구간 (CenterBounceMultiplier 적용 범위)
            Vector2 center = (_startPoint + _endPoint) * 0.5f;
            Vector2 direction = (_endPoint - _startPoint).normalized;
            float halfRange = Vector2.Distance(_startPoint, _endPoint) * _staticConfig.CenterThreshold;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(center - direction * halfRange, center + direction * halfRange);
        }
#endif
        #endregion
    }
}