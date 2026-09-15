using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Goal
{
    /// <summary>
    /// GoalPort의 시각 표현을 담당하는 클래스.
    /// </summary>
    public class GoalPortRenderer : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// 시각 표현을 적용할 준비가 되어 있는지 확인합니다.
        /// </summary>
        public bool CanApply => _spriteRenderer != null;
        #endregion


        #region Private Variables
        [SerializeField] private SpriteRenderer _spriteRenderer;
        #endregion


        #region Public Methods
        /// <summary>
        /// GoalPort의 시각 표현을 적용합니다.
        /// </summary>
        public void Apply(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }

        /// <summary>
        /// 표시 여부를 설정합니다.
        /// </summary>
        public void SetVisible(bool isVisible)
        {
            _spriteRenderer.enabled = isVisible;
        }
        #endregion
    }
}
