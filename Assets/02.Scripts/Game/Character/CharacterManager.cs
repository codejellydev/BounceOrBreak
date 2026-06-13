using CodeJelly.BounceOrBreak.Game.Bounce;
using CodeJelly.BounceOrBreak.Game.Reflect;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Character
{
    /// <summary>
    /// Character 관련 핵심 컴포넌트들을 통합 관리하는 Manager.
    /// 외부와의 상호작용 및 내부 컴포넌트들로의 작업 분배 수행. (Facade)
    /// </summary>
    [RequireComponent(typeof(CharacterCoordinator))]
    public class CharacterManager : MonoBehaviour, ICharacter
    {
        #region Properties
        /// <summary>
        /// ICharacter.Transform Implementation
        /// </summary>
        public Transform Transform => transform;
        #endregion


        #region Private Variables
        [Header("Character Components")]
        [SerializeField] private CharacterCoordinator _coordinator;
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
        /// ICharacter.ResetState Implementation
        /// </summary>
        public void ResetState()
        {
            _coordinator.ResetState();
        }

        /// <summary>
        /// IBounceable.Bounce Implementation 
        /// </summary>
        public void Bounce(BounceParams parameters)
        {
            _coordinator.HandleBounce(parameters);
        }

        /// <summary>
        /// IBounceable.OnBounced Implementation 
        /// </summary>
        public void OnBounced(BounceResult result)
        {
            _coordinator.HandleBounceResult(result);
        }

        /// <summary>
        /// IReflectable.Reflect Implementation
        /// </summary>
        public void Reflect(ReflectParams parameters)
        {
            _coordinator.HandleReflect(parameters);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            // Coordinator Initialization
            _coordinator ??= GetComponent<CharacterCoordinator>();
            _coordinator.Initialize();
        }
        #endregion
    }
}
