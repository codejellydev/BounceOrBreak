using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Character
{
    /// <summary>
    /// Character의 제어 로직을 담당하는 Presenter 클래스.
    /// </summary>
    public class CharacterPresenter : MonoBehaviour
    {
        #region Private Variables
        [Header("Character Components")]
        [SerializeField] private CharacterPhysics _physics;
        #endregion


        #region Public Methods
        public void Initialize()
        {
            InitializeComponents();
        }
        #endregion


        #region Private Methods
        private void InitializeComponents()
        {
            _physics ??= GetComponentInParent<CharacterPhysics>();
        }
        #endregion
    }
}
