using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using R3;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Goal
{
    /// <summary>
    /// Egg의 도달을 감지하는 목표 지점 클래스.
    /// 도달 사실과 타입 일치 여부만 발행하며, 득점 여부 판단은 구독 클래스가 처리합니다.
    /// TODO: IEgg 인터페이스로 추후 변경 예정 (ICharacter는 타입 정보가 없어 대체 불가)
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GoalPort : MonoBehaviour
    {
        #region Properties
        public EggType TargetType => _targetType;

        /// <summary>
        /// Egg가 도달했을 때 발행
        /// </summary>
        public Observable<GoalArrival> OnEggArrived => _onEggArrived;
        #endregion


        #region Private Variables
        [Tooltip("요구하는 계란 타입")]
        [SerializeField] private EggType _targetType;

        private readonly Subject<GoalArrival> _onEggArrived = new Subject<GoalArrival>();

        private Collider2D _collider;
        #endregion


        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<EggController>(out var egg) == false)
            {
                return;
            }

            // Data가 없으면 스폰이 완료되지 않은 Egg이므로 판정에서 제외
            if (egg.Data == null)
            {
                return;
            }

            GoalArrival arrival = new GoalArrival
            {
                Port = this,
                Egg = egg,
                IsMatched = egg.Data.Type == _targetType
            };

            AppLogger.Debug($"[GoalPort] Egg arrived: {egg.Data.Type}, matched={arrival.IsMatched}.");

            _onEggArrived.OnNext(arrival);
        }

        private void OnDestroy()
        {
            _onEggArrived.Dispose();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            _collider ??= GetComponent<Collider2D>();

            // non-trigger면 충돌체를 물리적으로 막아 통과 감지가 불가능하므로 강제 trigger 설정
            _collider.isTrigger = true;
        }
        #endregion
    }
}