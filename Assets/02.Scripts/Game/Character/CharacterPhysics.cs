using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Bounce;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Character
{
    /// <summary>
    /// Character의 물리 로직을 담당하는 클래스.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public class CharacterPhysics : MonoBehaviour
    {
        #region Properties
        public Rigidbody2D Rigidbody
        {
            get
            {
                if (_rigidbody == null)
                {
                    AppLogger.Warning($"[CharacterPhysics] Rigidbody not initialized, initializing.");
                    InitializeComponents();
                }

                return _rigidbody;
            }
        }
        #endregion


        #region Private Variables
        [SerializeField] private CircleCollider2D _collider;
        [SerializeField] private Rigidbody2D _rigidbody;
        #endregion


        #region Public Methods
        /// <summary>
        /// 클래스를 사용 가능한 상태로 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            InitializeComponents();
        }

        /// <summary>
        /// Bounce 파라미터를 받아 물리적 튕김을 적용합니다.
        /// </summary>
        public void ApplyBounce(BounceParams parameters)
        {
            float currentSpeed = _rigidbody.linearVelocity.magnitude; // 현재 속력
            float speedMultiplier = GetSpeedMultiplier(currentSpeed); // 현재 속력 기반 보정
            float adjustedForce = parameters.Force * speedMultiplier; // 계산된 힘

            adjustedForce = Mathf.Clamp
            (
                adjustedForce,
                GameConstants.Character.MIN_BOUNCE_FORCE,
                GameConstants.Character.MAX_BOUNCE_FORCE
            );

            Vector2 direction = parameters.Direction;

            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.AddForce(direction * adjustedForce, ForceMode2D.Impulse);

            AppLogger.Debug($"[CharacterPhysics] Bounced: direction={direction}, force={parameters.Force}");
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            // Collider Initialization
            _collider ??= GetComponent<CircleCollider2D>();

            // Rigidbody Initialization
            _rigidbody ??= GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// 현재 속력에 따른 튕김 힘 보정 배율을 계산합니다.
        /// 속력이 빠를수록 더 높은 배율을 반환합니다.
        /// </summary>
        /// <param name="currentSpeed">현재 속력 (m/s)</param>
        /// <returns>튕김 힘 보정 배율 (1.0 이상)</returns>
        private float GetSpeedMultiplier(float currentSpeed)
        {
            return 1f + (currentSpeed * GameConstants.Character.SPEED_BOUNCE_MULTIPLIER);
        }
        #endregion
    }
}