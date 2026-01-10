using CodeJelly.BounceOrBreak.Game.Bounce;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Character
{
    /// <summary>
    /// Character 관련 핵심 컴포넌트들을 통합 관리하는 Manager.
    /// 외부와의 상호작용 및 내부 컴포넌트들로의 작업 분배 수행. (Facade)
    /// </summary>
    [RequireComponent(typeof(CharacterPresenter))]
    public class CharacterManager : MonoBehaviour, ICharacter
    {
        #region Properties
        public Transform Transform => transform;

        public Rigidbody2D Rigidbody => _presenter.Physics.Rigidbody;
        #endregion


        #region Private Variables
        [Header("Character Components")]
        [SerializeField] private CharacterPresenter _presenter;
        #endregion


        #region Public Methods
        /// <summary>
        /// 클래스를 사용 가능한 상태로 초기화합니다.
        /// </summary>
        public void Initialize()
        {
            InitializeComponents();
        }

        // IBounceable Implementation
        public void Bounce(BounceParams parameters)
        {
            _presenter.HandleBounce(parameters);
        }

        public void OnBounced(BounceResult result)
        {
            _presenter.HandleBounceResult(result);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            // Presenter Initialization
            _presenter ??= GetComponent<CharacterPresenter>();
            _presenter.Initialize();
        }
        #endregion
    }
}
