using CodeJelly.BounceOrBreak.Game.Score;
using CodeJelly.BounceOrBreak.Game.UI.Widgets;
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
        #region Properties
        public PushButton PauseButton => _pauseButton;

        /// <summary>
        /// View가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _currentScoreText != null && _pauseButton != null;
        #endregion


        #region Private Variables
        [Header("Score")]
        [SerializeField] private TMP_Text _currentScoreText;

        [Header("Buttons")]
        [SerializeField] private PushButton _pauseButton;
        #endregion


        #region Public Methods
        /// <summary>
        /// 현재 점수를 표시에 반영합니다.
        /// </summary>
        public void UpdateCurrentScore(int score)
        {
            _currentScoreText.text = ScoreFormatter.Format(score);
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
