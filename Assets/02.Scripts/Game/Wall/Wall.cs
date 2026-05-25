using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Reflect;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Wall
{
    /// <summary>
    /// 씬에 배치되는 벽 객체를 관리하는 클래스.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Wall : MonoBehaviour
    {
        #region Private Variables
        [SerializeField] private WallConfig _config;

        private Collider2D _collider;
        #endregion


        #region Unity Lifecycle
        private void Awake()
        {
            Initialize();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<IReflectable>(out var reflectable) == false)
            {
                return;
            }

            // 반사 적용
            Vector2 contactNormal = collision.GetContact(0).normal;
            Vector2 velocityBeforeCollision = collision.relativeVelocity;

            reflectable.Reflect(new ReflectParams
            {
                Normal = contactNormal,
                Damping = _config.ReflectionDamping,
                VelocityBeforeCollision = velocityBeforeCollision
            });

            AppLogger.Debug($"[Wall] Reflected: velocity={velocityBeforeCollision}, normal={contactNormal}.");
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Wall을 초기화합니다.
        /// </summary>
        private void Initialize()
        {
            InitializeComponents();
        }

        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            _collider ??= GetComponent<Collider2D>();

            if (_config == null)
            {
                AppLogger.Error("[Wall] Config ScriptableObject is missing!");
            }
        }
        #endregion
    }
}