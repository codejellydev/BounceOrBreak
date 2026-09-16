using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Bounce;
using CodeJelly.BounceOrBreak.Game.Character;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// Egg의 도메인 로직을 담당하는 클래스.
    /// </summary>
    public class EggController : MonoBehaviour, IEgg
    {
        #region Properties
        /// <summary>
        /// IEgg.Data Implementation
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
        /// IEgg.Initialize Implementation
        /// TODO: data 기반 IEggBehavior 장착
        /// </summary>
        /// <param name="data">적용할 타입 데이터</param>
        /// <param name="position">배치할 위치</param>
        /// <returns>초기화 성공 시 true</returns>
        public bool Initialize(EggData data, Vector2 position)
        {
            if (data == null)
            {
                AppLogger.Error("[EggController] EggData is null.");

                return false;
            }

            Data = data;

            _renderer.ApplyData(data);
            transform.position = position;

            gameObject.SetActive(true);

            return true;
        }

        /// <summary>
        /// IEgg.Hold Implementation
        /// </summary>
        public void Hold(Vector2 position)
        {
            _character.SetMotionLocked(true);
            transform.position = position;
        }

        /// <summary>
        /// IEgg.Eject Implementation
        /// </summary>
        public void Eject(Vector2 direction, float force)
        {
            _character.SetMotionLocked(false);
            _character.Bounce(new BounceParams
            {
                Normal = direction,
                Force = force,
                VelocityBeforeCollision = Vector2.zero
            });
        }

        /// <summary>
        /// IPauseable.Pause Implementation
        /// </summary>
        public void Pause()
        {
            _character.Pause();
        }

        /// <summary>
        /// IPauseable.Resume Implementation
        /// </summary>
        public void Resume()
        {
            _character.Resume();
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