using CodeJelly.BounceOrBreak.Game.Bounce;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Character
{
    /// <summary>
    /// Character 관련 핵심 컴포넌트들을 통합 관리하는 Manager.
    /// 외부와의 상호작용 및 내부 컴포넌트들로의 작업 분배 수행. (Facade)
    /// </summary>
    public class CharacterManager : MonoBehaviour, ICharacter
    {
        #region Properties
        public Transform Transform => transform;

        public Rigidbody2D Rigidbody { get; private set; }
        #endregion


        #region Private Variables
        [Header("Character Components")]
        [SerializeField] private CharacterPresenter _presenter;
        #endregion


        #region Public Methods
        // IBounceable Implementation
        public void Bounce(BounceParams parameters)
        {
            throw new System.NotImplementedException();
        }

        public void OnBounced(BounceResult result)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
