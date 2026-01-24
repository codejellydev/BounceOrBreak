using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Bounce;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// Trampoline의 추상 클래스.
    /// </summary>
    [RequireComponent(typeof(LineRenderer), typeof(EdgeCollider2D))]
    public abstract class TrampolineBase : MonoBehaviour, ITrampoline
    {
        #region Nested Classes
        /// <summary>
        /// IBounceable과의 접촉 지점에 대한 계산 결과.
        /// </summary>
        protected struct ContactInfo
        {
            /// <summary>
            /// 접촉 지점
            /// </summary>
            public Vector2 ContactPoint { get; set; }

            /// <summary>
            /// 접촉 지점으로부터 가장 가까운 선분의 인덱스
            /// </summary>
            public int ClosestSegmentIndex { get; set; }

            /// <summary>
            /// 라인의 시작점(0번 인덱스)부터 접촉 지점까지의 거리
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
        public abstract TrampolineType Type { get; }

        /// <summary>
        /// ITrampoline.UsageCount Implementation
        /// </summary>
        public int UsageCount { get; protected set; }

        /// <summary>
        /// ITrampoline.Lifetime Implementation
        /// </summary>
        public float Lifetime { get; protected set; }
        #endregion


        #region Protected Variables
        protected TrampolineDrawer _drawer;
        protected TrampolineConfig _config;

        protected EdgeCollider2D _edgeCollider;
        protected LineRenderer _lineRenderer;

        protected bool _isInitialized;

        protected float _lineLength;
        #endregion


        #region Unity Lifecycle
        protected virtual void Update()
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

        protected virtual void OnCollisionEnter2D(Collision2D collision)
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
        public abstract void Initialize(TrampolineDrawer drawer, TrampolineConfig config, Vector2[] points);

        /// <summary>
        /// IPoolable.Get Implementation
        /// </summary>
        public virtual void Get()
        {
            UsageCount = 0;
            Lifetime = 0f;
            _isInitialized = false;

            gameObject.SetActive(true);
        }

        /// <summary>
        /// IPoolable.Release Implementation
        /// </summary>
        public virtual void Release()
        {
            _lineLength = 0f;
            _isInitialized = false;

            gameObject.SetActive(false);
        }

        /// <summary>
        /// IPoolable.Destroy Implementation
        /// </summary>
        public virtual void Destroy()
        {
            _config = null;
        }
        #endregion


        #region Protected Methods
        /// <summary>
        /// LineRenderer를 설정합니다.
        /// </summary>
        protected abstract void SetupLineRenderer();

        /// <summary>
        /// EdgeCollider2D를 설정합니다.
        /// </summary>
        protected abstract void SetupCollider();

        /// <summary>
        /// 라인의 특정 지점에서의 접촉 정보를 계산합니다.
        /// </summary>
        protected abstract ContactInfo AnalyzeContactInfo(Vector2 contactPoint);
       
         /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        protected virtual void InitializeComponents()
        {
            // LineRenderer Initialization
            _lineRenderer ??= GetComponent<LineRenderer>();

            // EdgeCollider2D Initialization
            _edgeCollider ??= GetComponent<EdgeCollider2D>();
        }

        /// <summary>
        /// 튕김 처리를 수행합니다.
        /// </summary>
        protected virtual void ProcessBounce(IBounceable bounceable, Vector2 contactPoint)
        {
            // 접촉 정보 분석
            ContactInfo contactInfo = AnalyzeContactInfo(contactPoint);

            bool isCenter = IsContactAtCenter(contactInfo);

            float bounceForce = isCenter
                ? _config.BounceForce * _config.CenterBounceMultiplier
                : _config.BounceForce;

            bounceable.Bounce(new BounceParams
            {
                Direction = contactInfo.Normal,
                Force = bounceForce
            });

            bounceable.OnBounced(new BounceResult
            {
                Energy = bounceForce,
                IsCenter = isCenter,
                ContactPoint = contactInfo.ContactPoint
            });

            AppLogger.Debug($"[{Type}Trampoline] Bounce: force={bounceForce}, isCenter={isCenter}, contactPoint={contactPoint}.");
        }

        /// <summary>
        /// 접촉 지점이 Trampoline 중앙 범위 내에 있는지 판정합니다.
        /// </summary>
        /// <param name="contactInfo">접촉 정보</param>
        /// <returns>중앙 범위 내이면 true</returns>
        protected bool IsContactAtCenter(ContactInfo contactInfo)
        {
            float centerRatio = contactInfo.DistanceFromStart / _lineLength;

            // 중앙 판정
            float lowerBound = 0.5f - _config.CenterThreshold;
            float upperBound = 0.5f + _config.CenterThreshold;

            return (lowerBound <= centerRatio) && (centerRatio <= upperBound);
        }

        /// <summary>
        /// Trampoline을 비활성화하고 Pool에 반환합니다.
        /// </summary>
        protected virtual void Despawn()
        {
            if (_isInitialized == false)
            {
                return;
            }

            _isInitialized = false;
            AppLogger.Info($"[{Type}Trampoline] Despawned. UsageCount: {UsageCount}, Lifetime: {Lifetime:F2}s");

            _drawer?.ReturnToPool(this);
        }
        #endregion
    }
}