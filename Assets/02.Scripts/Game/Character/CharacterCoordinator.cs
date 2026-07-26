using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Bounce;
using CodeJelly.BounceOrBreak.Game.Reflect;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Character
{
    /// <summary>
    /// Character의 제어 로직을 담당하는 Coordinator 클래스.
    /// </summary>
    public class CharacterCoordinator : MonoBehaviour
    {
        #region Private Variables
        [Header("Character Components")]
        [SerializeField] private CharacterPhysics _physics;
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
        /// 캐릭터 상태를 초기화한다.
        /// </summary>
        public void ResetState()
        {
            _physics?.ResetState();
        }

        /// <summary>
        /// Bounce 이벤트를 처리합니다.
        /// </summary>
        public void HandleBounce(BounceParams parameters)
        {
            _physics?.ApplyBounce(parameters);
        }

        /// <summary>
        /// Bounce 이벤트에 따른 결과를 처리합니다.
        /// </summary>
        public void HandleBounceResult(BounceResult result)
        {
            // TODO: Effect 적용 등 튕김에 따른 후속 처리 구현
        }

        /// <summary>
        /// Reflect 이벤트를 처리합니다.
        /// </summary>
        public void HandleReflect(ReflectParams parameters)
        {
            _physics?.ApplyReflect(parameters);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            // Physics Initialization
            _physics ??= GetComponent<CharacterPhysics>();
            if (_physics == null)
            {
                AppLogger.Error("[CharacterCoordinator] CharacterPhysics component is missing.");

                return;
            }
            _physics.Initialize();
        }
        #endregion
    }
}