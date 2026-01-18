using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Bounce;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// 시작점과 끝점을 직선으로 연결한 일자형 Trampoline.
    /// </summary>
    [RequireComponent(typeof(LineRenderer), typeof(EdgeCollider2D))]
    public class StraightTrampoline : MonoBehaviour, ITrampoline
    {
        #region Properties
        /// <summary>
        /// ITrampoline.Type Implementation
        /// </summary>
        public TrampolineType Type => TrampolineType.Straight;

        /// <summary>
        /// ITrampoline.UsageCount Implementation
        /// </summary>
        public int UsageCount { get; private set; }

        /// <summary>
        /// ITrampoline.Lifetime Implementation
        /// </summary>
        public float Lifetime { get; private set; }
        #endregion


        #region Private Variables
        private TrampolineDrawer _drawer;
        private TrampolineConfig _config;

        private EdgeCollider2D _edgeCollider;
        private LineRenderer _lineRenderer;

        private Vector2 _startPoint;
        private Vector2 _endPoint;
        private float _lineLength;
        private Vector2 _lineDirection;
        private Vector2 _normal;

        private bool _isInitialized;
        #endregion


        #region Unity Lifecycle
        private void Update()
        {
            if (_isInitialized == false)
            {
                return;
            }

            Lifetime += Time.deltaTime;

            if (_config.MaxLifetime > 0f && Lifetime >= _config.MaxLifetime)
            {
                Despawn();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_isInitialized == false)
            {
                return;
            }

            var bounceable = collision.gameObject.GetComponent<IBounceable>();
            if (bounceable == null)
            {
                return;
            }

            // 접촉 지점 계산
            Vector2 contactPoint = collision.GetContact(0).point;
            ProcessBounce(bounceable, contactPoint);

            UsageCount++;

            if (0 < _config.MaxUsageCount && _config.MaxUsageCount <= UsageCount)
            {
                Despawn();
            }
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// ITrampoline.Initialize Implementation
        /// </summary>
        /// <param name="drawer">Trampoline을 생성한 Drawer</param>
        /// <param name="config">Trampoline 설정</param>
        /// <param name="points">LineRenderer의 point 배열 (시작점, 끝점 2개)</param>
        public void Initialize(TrampolineDrawer drawer, TrampolineConfig config, Vector2[] points)
        {
            if (points.Length != 2)
            {
                AppLogger.Error($"[StraightTrampoline] Invalid points count: {points.Length}. Expected 2.");

                return;
            }

            _drawer = drawer;
            _config = config;
            _startPoint = points[0];
            _endPoint = points[1];

            InitializeComponents();
            CalculateLineProperties();
            SetupLineRenderer();
            SetupCollider();

            UsageCount = 0;
            Lifetime = 0f;

            _isInitialized = true;

            AppLogger.Debug("[StraightTrampoline] Initialized.");
        }

        /// <summary>
        /// IPoolable.Get Implementation
        /// </summary>
        public void Get()
        {
            UsageCount = 0;
            Lifetime = 0f;
            _isInitialized = false;

            gameObject.SetActive(true);
        }

        /// <summary>
        /// IPoolable.Release Implementation
        /// </summary>
        public void Release()
        {
            _lineLength = 0f;
            _isInitialized = false;

            gameObject.SetActive(false);
        }

        /// <summary>
        /// IPoolable.Destroy Implementation
        /// </summary>
        public void Destroy()
        {
            _config = null;
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            // LineRenderer Initialization
            _lineRenderer ??= GetComponent<LineRenderer>();

            // EdgeCollider2D Initialization
            _edgeCollider ??= GetComponent<EdgeCollider2D>();
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

        /// <summary>
        /// LineRenderer를 설정합니다.
        /// </summary>
        private void SetupLineRenderer()
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
        private void SetupCollider()
        {
            // EdgeCollider2D는 Local Space를 사용하므로 World Space -> Local Space 좌표 변환 수행
            Vector2[] localPoints = new Vector2[2];
            localPoints[0] = transform.InverseTransformPoint(_startPoint);
            localPoints[1] = transform.InverseTransformPoint(_endPoint);

            _edgeCollider.points = localPoints;
        }

        /// <summary>
        /// Trampoline을 비활성화하고 Pool에 반환합니다.
        /// </summary>
        private void Despawn()
        {
            if (_isInitialized == false)
            {
                return;
            }

            _isInitialized = false;
            AppLogger.Info($"[StraightTrampoline] Despawned. UsageCount: {UsageCount}, Lifetime: {Lifetime:F2}s");

            _drawer?.ReturnToPool(this);
        }

        /// <summary>
        /// 튕김 처리를 수행합니다.
        /// </summary>
        private void ProcessBounce(IBounceable bounceable, Vector2 contactPoint)
        {
            // 중앙 판정
            bool isCenter = IsCenter(contactPoint);

            // 튕김 힘 설정
            float bounceForce = isCenter
                ? _config.BounceForce * _config.CenterBounceMultiplier
                : _config.BounceForce;

            // IBounceable.Bounce
            bounceable.Bounce(new BounceParams
            {
                Direction = _normal,
                Force = bounceForce,
                ContactPoint = contactPoint
            });

            // IBounceable.OnBounced
            bounceable.OnBounced(new BounceResult
            {
                Energy = bounceForce,
                IsCenter = isCenter
            });

            AppLogger.Debug($"[StraightTrampoline] Bounce: force={bounceForce}, isCenter={isCenter}");
        }

        /// <summary>
        /// 충돌 지점이 라인의 중앙 범위 내에 있는지 판정합니다.
        /// </summary>
        private bool IsCenter(Vector2 contactPoint)
        {
            float distanceFromStart = Vector2.Distance(_startPoint, contactPoint);
            float centerRatio = distanceFromStart / _lineLength;

            // 중앙 판정
            float lowerBound = 0.5f - _config.CenterThreshold;
            float upperBound = 0.5f + _config.CenterThreshold;

            return (lowerBound <= centerRatio) && (centerRatio <= upperBound);
        }
        #endregion
    }
}