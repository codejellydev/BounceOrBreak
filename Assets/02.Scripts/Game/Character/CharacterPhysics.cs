using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Bounce;
using CodeJelly.BounceOrBreak.Game.Reflect;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Character
{
    /// <summary>
    /// Character의 물리 로직을 담당하는 클래스.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public class CharacterPhysics : MonoBehaviour
    {
        #region Private Variables
        [SerializeField] private CircleCollider2D _collider;
        [SerializeField] private Rigidbody2D _rigidbody;

        private bool _isPaused;
        private Vector2 _pausedVelocity;
        private float _pausedAngularVelocity;
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
        /// 물리 상태를 초기화합니다.
        /// </summary>
        public void ResetState()
        {
            // Rigidbody 잔여 속도 해제
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;

            // SetMotionLocked(true)로 고정된 채 회수되어도 초기 상태로 복원
            _rigidbody.constraints = RigidbodyConstraints2D.None;

            // 정지된 채 회수되어도 초기 상태로 복원
            _isPaused = false;
            _rigidbody.simulated = true;
        }

        /// <summary>
        /// 물리적 움직임의 고정 여부를 설정합니다.
        /// </summary>
        public void SetMotionLocked(bool isLocked)
        {
            if (isLocked == true)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.angularVelocity = 0f;
                _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;

                return;
            }

            _rigidbody.constraints = RigidbodyConstraints2D.None;
        }

        /// <summary>
        /// 물리 시뮬레이션을 멈춥니다. 이미 멈춰 있으면 무시합니다.
        /// </summary>
        public void Pause()
        {
            if (_isPaused == true)
            {
                return;
            }

            _isPaused = true;
            _pausedVelocity = _rigidbody.linearVelocity;
            _pausedAngularVelocity = _rigidbody.angularVelocity;
            _rigidbody.simulated = false;
        }

        /// <summary>
        /// 멈춘 물리 시뮬레이션을 이어갑니다. 멈춰 있지 않으면 무시합니다.
        /// </summary>
        public void Resume()
        {
            if (_isPaused == false)
            {
                return;
            }

            _isPaused = false;
            _rigidbody.simulated = true;
            _rigidbody.linearVelocity = _pausedVelocity;
            _rigidbody.angularVelocity = _pausedAngularVelocity;
        }

        /// <summary>
        /// Bounce 파라미터를 받아 물리적 튕김을 적용합니다.
        /// </summary>
        public void ApplyBounce(BounceParams parameters)
        {
            Vector2 velocityBeforeCollision = parameters.VelocityBeforeCollision; // 전달받은 충돌 전 속도
            float currentSpeed = velocityBeforeCollision.magnitude; // 전달받은 충돌 전 속력
            float speedMultiplier = GetSpeedMultiplier(currentSpeed); // 현재 속력 기반 보정
            float adjustedForce = parameters.Force * speedMultiplier; // 계산된 힘

            adjustedForce = Mathf.Clamp
            (
                adjustedForce,
                GameConstants.Character.MIN_BOUNCE_FORCE,
                GameConstants.Character.MAX_BOUNCE_FORCE
            );

            Vector2 direction;
            if (currentSpeed > GameConstants.Character.MIN_VELOCITY_FOR_REFLECT)
            {
                Vector2 reflected = Vector2.Reflect(velocityBeforeCollision.normalized, parameters.Normal).normalized;
                direction = Vector2.Lerp(reflected, parameters.Normal, parameters.NormalBias).normalized; // 노말 방향 가중치 고려하여 디렉션 블렌딩
            }
            else
            {
                direction = parameters.Normal;
            }

            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.AddForce(direction * adjustedForce, ForceMode2D.Impulse);

            AppLogger.Debug($"[CharacterPhysics] Bounced: direction={direction}, force={parameters.Force}");
        }

        /// <summary>
        /// Reflect 파라미터를 받아 물리적 반사를 적용합니다.
        /// </summary>
        public void ApplyReflect(ReflectParams parameters)
        {
            // 전달받은 충돌 전 속도를 사용
            Vector2 reflectedVelocity = Vector2.Reflect(parameters.VelocityBeforeCollision, parameters.Normal);

            // 감쇠 적용
            _rigidbody.linearVelocity = reflectedVelocity * parameters.Damping;

            AppLogger.Debug($"[CharacterPhysics] Reflected: {parameters.VelocityBeforeCollision} -> {_rigidbody.linearVelocity}");
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            // Collider Initialization
            if (_collider == null)
            {
                _collider = GetComponent<CircleCollider2D>();
            }

            // Rigidbody Initialization
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody2D>();
            }
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