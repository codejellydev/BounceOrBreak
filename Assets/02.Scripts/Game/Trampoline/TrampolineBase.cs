using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Pause;
using CodeJelly.BounceOrBreak.Game.Bounce;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// Trampoline의 추상 클래스.
    /// </summary>
    [RequireComponent(typeof(EdgeCollider2D), typeof(PlatformEffector2D))]
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
        public float Lifetime => (_isInitialized == true)
                              ? _lifetimeStopwatch.GetElapsed()
                              : 0f;

        /// <summary>
        /// Trampoline이 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _renderer != null && _renderer.CanApply;
        #endregion


        #region Protected Variables
        protected TrampolineDrawer _drawer;
        protected TrampolineConfig _config;

        protected EdgeCollider2D _edgeCollider;
        protected PlatformEffector2D _platformEffector;

        protected bool _isInitialized;

        protected float _lineLength;
        #endregion


        #region Private Variables
        [SerializeField] private TrampolineRenderer _renderer;

        private readonly PausableStopwatch _lifetimeStopwatch = new PausableStopwatch();

        private CancellationTokenSource _lifetimeCts;
        #endregion


        #region Unity Lifecycle
        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (_isInitialized == false)
            {
                return;
            }

            if (collision.gameObject.TryGetComponent<IBounceable>(out var bounceable) == false)
            {
                return;
            }

            Vector2 relativeVelocity = collision.relativeVelocity;
            if (relativeVelocity.y > 0f) // 위로 올라가는 중이면 무시
            {
                return;
            }

            // 접촉 지점 계산
            Vector2 contactPoint = collision.GetContact(0).point;
            ProcessBounce(bounceable, contactPoint, relativeVelocity);

            UsageCount++;

            if (0 < _config.MaxUsageCount && _config.MaxUsageCount <= UsageCount)
            {
                Despawn();

                return;
            }

            _renderer.SetMatSprite(GetCurrentMatSprite());
        }

        protected virtual void OnDestroy()
        {
            CancelLifetimeTimer();
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// ITrampoline.Initialize Implementation
        /// </summary>
        /// <param name="drawer">Trampoline을 생성한 Drawer</param>
        /// <param name="config">Trampoline 설정</param>
        /// <param name="points">LineRenderer의 point 배열</param>
        /// <returns>초기화 성공 시 true</returns>
        public abstract bool Initialize(TrampolineDrawer drawer, TrampolineConfig config, Vector2[] points);

        /// <summary>
        /// IPoolable.Get Implementation
        /// </summary>
        public virtual void Get()
        { }

        /// <summary>
        /// IPoolable.Release Implementation
        /// </summary>
        public virtual void Release()
        {
            CancelLifetimeTimer();

            UsageCount = 0;
            _lineLength = 0f;
            _isInitialized = false;

            // 정지된 채 회수되어도 다음 수명 타이머가 흐르도록 복원
            _lifetimeStopwatch.Resume();

            // 씬 해체 중에는 이미 파괴되었을 수 있음
            if (this == null)
            {
                return;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// IPoolable.Destroy Implementation
        /// </summary>
        public virtual void Destroy()
        {
            CancelLifetimeTimer();

            _config = null;

            // 씬 해체 중에는 이미 파괴되었을 수 있음
            if (this == null)
            {
                return;
            }

            GameObject.Destroy(gameObject);
        }

        /// <summary>
        /// IPauseable.Pause Implementation
        /// </summary>
        public void Pause()
        {
            _lifetimeStopwatch.Pause();
        }

        /// <summary>
        /// IPauseable.Resume Implementation
        /// </summary>
        public void Resume()
        {
            _lifetimeStopwatch.Resume();
        }
        #endregion


        #region Protected Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        protected virtual void InitializeComponents()
        {
            // EdgeCollider2D Initialization
            _edgeCollider ??= GetComponent<EdgeCollider2D>();

            // PlatformEffector2D Initialization
            _platformEffector ??= GetComponent<PlatformEffector2D>();
        }

        /// <summary>
        /// EdgeCollider2D를 설정합니다.
        /// </summary>
        protected abstract void SetupCollider();

        /// <summary>
        /// PlatformEffector를 설정합니다.
        /// </summary>
        protected virtual void SetupPlatformEffector()
        {
            // Platform Effector 설정
            _edgeCollider.usedByEffector = true;

            _platformEffector.useOneWay = true;
            _platformEffector.surfaceArc = 180f;
            _platformEffector.rotationalOffset = 0f;

            // 측면 효과 비활성화
            _platformEffector.useSideFriction = false;
            _platformEffector.useSideBounce = false;
            _platformEffector.sideArc = 0f;
        }

        /// <summary>
        /// 시각 표현을 적용합니다.
        /// </summary>
        protected void ApplyVisual(Vector2[] points)
        {
            _renderer.Apply(points, _config, GetCurrentMatSprite());
        }

        /// <summary>
        /// Trampoline을 활성화하고 수명을 시작합니다.
        /// 구현체의 설정이 모두 끝난 뒤 호출해야 합니다.
        /// </summary>
        protected void Spawn()
        {
            _isInitialized = true;

            StartLifetimeTimer();

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Lifetime 타이머를 시작합니다.
        /// Initialize 완료 후 호출해야 합니다.
        /// </summary>
        protected void StartLifetimeTimer()
        {
            CancelLifetimeTimer();

            _lifetimeStopwatch.Start();
            _lifetimeCts = new CancellationTokenSource();
            RunLifetimeTimer(_lifetimeCts.Token).Forget();
        }

        /// <summary>
        /// Lifetime 타이머를 중지합니다.
        /// </summary>
        protected void CancelLifetimeTimer()
        {
            _lifetimeCts?.Cancel();
            _lifetimeCts?.Dispose();

            _lifetimeCts = null;
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

            AppLogger.Info($"[{Type}Trampoline] Despawned. UsageCount: {UsageCount}, Lifetime: {Lifetime:F2}s");

            _isInitialized = false;
            CancelLifetimeTimer();

            _drawer?.ReturnToPool(this);
        }

        /// <summary>
        /// 라인의 특정 지점에서의 접촉 정보를 계산합니다.
        /// </summary>
        protected abstract ContactInfo AnalyzeContactInfo(Vector2 contactPoint);

        /// <summary>
        /// 튕김 처리를 수행합니다.
        /// </summary>
        protected virtual void ProcessBounce(IBounceable bounceable, Vector2 contactPoint, Vector2 velocityBeforeCollision)
        {
            // 접촉 정보 분석
            ContactInfo contactInfo = AnalyzeContactInfo(contactPoint);

            bool isCenter = IsContactAtCenter(contactInfo);

            float bounceForce = isCenter
                ? _config.BounceForce * _config.CenterBounceMultiplier
                : _config.BounceForce;

            bounceable.Bounce(new BounceParams
            {
                Normal = contactInfo.Normal,
                Force = bounceForce,
                VelocityBeforeCollision = velocityBeforeCollision,
                NormalBias = _config.NormalBias
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
            return (_config.CenterZoneStart <= centerRatio) && (centerRatio <= _config.CenterZoneEnd);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 남은 사용 횟수에 해당하는 매트 스프라이트를 반환합니다.
        /// </summary>
        private Sprite GetCurrentMatSprite()
        {
            return _config.GetMatSprite(_config.MaxUsageCount - UsageCount);
        }

        /// <summary>
        /// MaxLifetime 경과 시 Trampoline을 Despawn합니다.
        /// </summary>
        private async UniTaskVoid RunLifetimeTimer(CancellationToken ct)
        {
            if (_config.MaxLifetime <= 0f)
            {
                return;
            }

            await _lifetimeStopwatch.WaitForSecondsAsync(_config.MaxLifetime, ct);
            Despawn();
        }
        #endregion
    }
}