using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Bounce;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// 그린 모양 그대로 유지되는 자유형 Trampoline.
    /// </summary>
    [RequireComponent(typeof(LineRenderer), typeof(EdgeCollider2D))]
    public class FreeformTrampoline : MonoBehaviour, ITrampoline
    {
        #region Nested Classes
        /// <summary>
        /// IBounceable과의 충돌 지점에 대한 계산 결과.
        /// </summary>
        private struct ContactInfo
        {
            /// <summary>
            /// 충돌 지점으로부터 가장 가까운 선분의 인덱스
            /// </summary>
            public int ClosestSegmentIndex { get; set; }

            /// <summary>
            /// 라인의 시작점(0번 인덱스)부터 충돌 지점까지의 거리
            /// </summary>
            public float DistanceFromStart { get; set; }

            /// <summary>
            /// 노말 벡터
            /// </summary>
            public Vector2 Normal { get; set; }
        }
        #endregion


        #region Properties
        /// <summary>
        /// ITrampoline.Type Implementation
        /// </summary>
        public TrampolineType Type => TrampolineType.Freeform;

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
        private Vector2[] _linePoints;
        private float _lineLength;

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
        /// <param name="points">LineRenderer의 point 배열</param>
        public void Initialize(TrampolineDrawer drawer, TrampolineConfig config, Vector2[] points)
        {
            _drawer = drawer;
            _config = config;
            _linePoints = points;

            InitializeComponents();
            SetupLineRenderer();
            SetupCollider();
            CalculateLineLength();

            UsageCount = 0;
            Lifetime = 0f;

            _isInitialized = true;

            AppLogger.Debug($"[Trampoline] Initialized with {points.Length} points");
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
            _linePoints = null;
            _lineLength = 0f;
            _isInitialized = false;

            gameObject.SetActive(false);
        }

        /// <summary>
        /// IPoolable.Destroy Implementation
        /// </summary>
        public void Destroy()
        {
            _linePoints = null;
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
        /// LineRenderer를 설정합니다.
        /// </summary>
        private void SetupLineRenderer()
        {
            _lineRenderer.positionCount = _linePoints.Length;
            _lineRenderer.startWidth = _config.LineWidth;
            _lineRenderer.endWidth = _config.LineWidth;
            _lineRenderer.material = _config.LineMaterial;
            _lineRenderer.startColor = _config.LineColor;
            _lineRenderer.endColor = _config.LineColor;

            for (int i = 0; i < _linePoints.Length; i++)
            {
                _lineRenderer.SetPosition(i, _linePoints[i]);
            }
        }

        /// <summary>
        /// EdgeCollider2D를 설정합니다.
        /// </summary>
        private void SetupCollider()
        {
            // EdgeCollider2D는 Local Space를 사용하므로 World Space -> Local Space 좌표 변환 수행
            Vector2[] localPoints = new Vector2[_linePoints.Length];
            for (int i = 0; i < _linePoints.Length; i++)
            {
                localPoints[i] = transform.InverseTransformPoint(_linePoints[i]);
            }

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
            AppLogger.Info($"[Trampoline] Despawned. UsageCount: {UsageCount}, Lifetime: {Lifetime:F2}s");

            _drawer?.ReturnToPool(this);
        }

        /// <summary>
        /// 라인의 총 길이를 계산합니다. (중앙 계산 등에 사용)
        /// </summary>
        private void CalculateLineLength()
        {
            _lineLength = 0f;
            for (int i = 0; i < _linePoints.Length - 1; i++)
            {
                _lineLength += Vector2.Distance(_linePoints[i], _linePoints[i + 1]);
            }
        }

        /// <summary>
        /// 튕김 처리를 수행합니다.
        /// </summary>
        private void ProcessBounce(IBounceable bounceable, Vector2 contactPoint)
        {
            ContactInfo contactInfo = AnalyzeContactInfo(contactPoint);

            Vector2 normal = contactInfo.Normal;

            // 중앙 판정
            float centerRatio = contactInfo.DistanceFromStart / _lineLength;
            float lowerBound = 0.5f - _config.CenterThreshold;
            float upperBound = 0.5f + _config.CenterThreshold;
            bool isCenter = (lowerBound <= centerRatio) && (centerRatio <= upperBound);

            // 튕김 힘 설정
            float bounceForce = isCenter
                ? _config.BounceForce * _config.CenterBounceMultiplier
                : _config.BounceForce;

            // IBounceable.Bounce
            bounceable.Bounce(new BounceParams
            {
                Direction = normal,
                Force = bounceForce,
                ContactPoint = contactPoint
            });

            // IBounceable.OnBounced
            bounceable.OnBounced(new BounceResult
            {
                Energy = bounceForce,
                IsCenter = isCenter
            });

            AppLogger.Debug($"[Trampoline] Bounce: force={bounceForce}, isCenter={isCenter}");
        }

        /// <summary>
        /// 라인의 특정 지점에서의 충돌 정보를 계산합니다.
        /// </summary>
        private ContactInfo AnalyzeContactInfo(Vector2 contactPoint)
        {
            float minDistance = float.MaxValue;
            int closestSegmentIndex = 0;

            // point로부터 가장 가까운 선분 확인 (접촉 지점)
            float closestProjectionLength = 0f;
            for (int i = 0; i < _linePoints.Length - 1; i++)
            {
                float distance = GetDistanceToSegment(contactPoint, _linePoints[i], _linePoints[i + 1], out float projectionLength);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestSegmentIndex = i;
                    closestProjectionLength = projectionLength;
                }
            }

            // 시작점부터 현재 segment까지의 누적 거리 계산
            float distanceFromStart = 0f;
            for (int j = 0; j < closestSegmentIndex; j++)
            {
                distanceFromStart += Vector2.Distance(_linePoints[j], _linePoints[j + 1]);
            }
            distanceFromStart += closestProjectionLength;

            // 노말 벡터 계산
            Vector2 segmentDirection = (_linePoints[closestSegmentIndex + 1] - _linePoints[closestSegmentIndex]).normalized;
            Vector2 normal = new Vector2(-segmentDirection.y, segmentDirection.x); // 90도 반시계방향 회전

            // 노말 벡터는 항상 Upward를 향해야 함
            if (normal.y < 0f)
            {
                normal = -normal;
            }

            return new ContactInfo
            {
                ClosestSegmentIndex = closestSegmentIndex,
                DistanceFromStart = distanceFromStart,
                Normal = normal.normalized
            };
        }

        /// <summary>
        /// 점에서 선분까지의 최단 거리를 계산합니다.
        /// </summary>
        private float GetDistanceToSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd, out float projectionLength)
        {
            Vector2 segmentVector = segmentEnd - segmentStart;
            Vector2 pointVector = point - segmentStart;

            float segmentLength = segmentVector.magnitude;
            Vector2 segmentDirection = segmentVector / segmentLength; // projection 계산에 사용하기 위해 segmentLength를 이미 구했기 때문에 normalized 사용하지 않음.

            // Projection 수행
            projectionLength = Vector2.Dot(pointVector, segmentDirection);
            projectionLength = Mathf.Clamp(projectionLength, 0f, segmentLength);

            Vector2 closestPoint = segmentStart + segmentDirection * projectionLength;

            return Vector2.Distance(point, closestPoint);
        }
        #endregion
    }
}