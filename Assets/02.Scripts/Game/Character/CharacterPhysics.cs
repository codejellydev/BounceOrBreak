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
            Vector2 direction = parameters.Direction;

            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.AddForce(direction * parameters.Force, ForceMode2D.Impulse);

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
        #endregion
    }
}