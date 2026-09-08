using TMPro;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.UI
{
    /// <summary>
    /// GameScene HUD의 View 클래스.
    /// GameplayHudPresenter로부터 제어됩니다.
    /// </summary>
    public class GameplayHudView : MonoBehaviour
    {
        #region Private Variables
        [Header("Score")]
        [SerializeField] private TMP_Text _currentScoreText;
        #endregion


        #region Public Methods
        /// <summary>
        /// 현재 점수를 표시에 반영합니다.
        /// </summary>
        public void UpdateCurrentScore(int score)
        {
            _currentScoreText.text = score.ToString();
        }

        /// <summary>
        /// HUD의 표시 여부를 설정합니다.
        /// </summary>
        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }
        #endregion
    }
}
