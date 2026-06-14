using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Pooling;
using CodeJelly.BounceOrBreak.Game.Character;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// 계란 캐릭터의 도메인 로직을 담당하는 클래스.
    /// </summary>
    public class EggController : MonoBehaviour, IPoolable
    {
        #region Properties
        /// <summary>
        /// 계란의 타입 데이터.
        /// </summary>
        public EggTypeData Data { get; private set; }
        #endregion


        #region Private Variables
        private ICharacter _character;
        #endregion


        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// 계란을 지정된 타입 데이터로 초기화합니다.
        /// </summary>
        public void Initialize(EggTypeData data)
        {
            Data = data;
            // TODO: data 기반 IEggBehavior 장착
        }

        /// <summary>
        /// IPoolable.Get Implementation
        /// </summary>
        public void Get()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// IPoolable.Release Implementation
        /// </summary>
        public void Release()
        {
            _character.ResetState();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// IPoolable.Destroy Implementation
        /// </summary>
        public void Destroy()
        { }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            _character ??= GetComponent<ICharacter>();
            if (_character == null)
            {
                AppLogger.Error("[EggController] ICharacter component is missing.");
            }
        }
        #endregion
    }
}