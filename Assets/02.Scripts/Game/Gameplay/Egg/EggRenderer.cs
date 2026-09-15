using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// Egg의 시각 표현을 담당하는 클래스.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class EggRenderer : MonoBehaviour
    {
        #region Private Variables
        [SerializeField] private SpriteRenderer _spriteRenderer;
        #endregion


        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// EggData의 시각 정보를 반영합니다.
        /// </summary>
        public void ApplyData(EggData data)
        {
            _spriteRenderer.sprite = data.BaseSprite;
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }
        #endregion
    }
}
