using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Pooling;
using CodeJelly.BounceOrBreak.Game.Character;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// Egg의 도메인 로직을 담당하는 클래스.
    /// </summary>
    public class EggController : MonoBehaviour, IPoolable
    {
        #region Properties
        /// <summary>
        /// Egg의 타입 데이터.
        /// </summary>
        public EggData Data { get; private set; }
        #endregion


        #region Private Variables
        [SerializeField] private EggRenderer _renderer;

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
        /// Egg를 지정된 타입 데이터로 초기화하고 활성화합니다.
        /// TODO: data 기반 IEggBehavior 장착
        /// </summary>
        /// <returns>초기화 성공 시 true</returns>
        public bool Initialize(EggData data)
        {
            if (data == null)
            {
                AppLogger.Error("[EggController] EggData is null.");

                return false;
            }

            Data = data;

            _renderer.ApplyData(data);

            gameObject.SetActive(true);

            return true;
        }

        /// <summary>
        /// IPoolable.Get Implementation
        /// </summary>
        public void Get()
        { }

        /// <summary>
        /// IPoolable.Release Implementation
        /// </summary>
        public void Release()
        {
            Data = null;

            // 씬 해체 중에는 이미 파괴되었을 수 있음
            if (this == null)
            {
                return;
            }

            _character.ResetState();

            gameObject.SetActive(false);
        }

        /// <summary>
        /// IPoolable.Destroy Implementation
        /// </summary>
        public void Destroy()
        {
            // 씬 해체 중에는 이미 파괴되었을 수 있음
            if (this == null)
            {
                return;
            }

            GameObject.Destroy(gameObject);
        }
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

            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<EggRenderer>();

                if (_renderer == null)
                {
                    AppLogger.Error("[EggController] EggRenderer component is missing.");
                }
            }
        }
        #endregion
    }
}