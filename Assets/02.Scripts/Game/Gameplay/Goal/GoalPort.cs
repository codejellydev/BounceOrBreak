using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using Cysharp.Threading.Tasks;
using R3;
using System.Threading;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Goal
{
    /// <summary>
    /// Egg의 도달을 감지하는 목표 지점 클래스.
    /// 도달 사실과 EggColor 일치 여부를 발행하며, 득점 여부 판단은 구독 클래스가 처리합니다.
    /// TODO: IEgg 인터페이스로 추후 변경 예정 (ICharacter는 타입 정보가 없어 대체 불가)
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GoalPort : MonoBehaviour
    {
        #region Properties
        public EggColor TargetColor => _targetColor;

        /// <summary>
        /// Egg가 도달했을 때 발행
        /// </summary>
        public Observable<GoalArrival> OnEggArrived => _onEggArrived;
        #endregion


        #region Private Variables
        [Tooltip("요구하는 EggColor")]
        [SerializeField] private EggColor _targetColor;

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
                IsMatched = egg.Data.Color == _targetColor
            };

            AppLogger.Debug($"[GoalPort] Egg arrived: {egg.Data.Color}, matched={arrival.IsMatched}.");

            _onEggArrived.OnNext(arrival);
        }

        private void OnDestroy()
        {
            _onEggArrived.Dispose();
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// 거부된 Egg를 일정 시간 머금은 뒤 내보냅니다.
        /// </summary>
        public async UniTask RejectAsync(EggController egg, CancellationToken ct)
        {
            egg.Hold(transform.position);

            await UniTask.WaitForSeconds(GameConstants.Goal.REJECT_HOLD_DURATION, cancellationToken: ct);

            egg.Eject(GetRejectDirection(), GameConstants.Goal.REJECT_FORCE);
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

        /// <summary>
        /// Egg를 내보낼 방향을 결정합니다.
        /// +y 방향을 기준으로 좌우로 무작위하게 기울입니다.
        /// </summary>
        private Vector2 GetRejectDirection()
        {
            float angle = 90f + Random.Range(-GameConstants.Goal.REJECT_CONE_HALF_ANGLE, GameConstants.Goal.REJECT_CONE_HALF_ANGLE);
            float radian = angle * Mathf.Deg2Rad;

            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }
        #endregion
    }
}