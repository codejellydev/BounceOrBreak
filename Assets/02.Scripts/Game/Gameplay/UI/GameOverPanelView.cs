using CodeJelly.BounceOrBreak.Game.Score;
using CodeJelly.BounceOrBreak.Game.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.UI
{
    /// <summary>
    /// GameOver 패널의 View 클래스.
    /// GameOverPanelPresenter로부터 제어됩니다.
    /// </summary>
    public class GameOverPanelView : MonoBehaviour
    {
        #region Properties
        public PushButton RestartButton => _restartButton;
        public PushButton HomeButton => _homeButton;

        /// <summary>
        /// View가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _restartButton != null
                            && _homeButton != null
                            && _currentScoreText != null
                            && _bestScoreText != null
                            && _newBestBadge != null
                            && _brokenEggIcon != null;
        #endregion


        #region Private Variables
        [Header("Score")]
        [SerializeField] private Image _brokenEggIcon;
        [SerializeField] private TMP_Text _currentScoreText;
        [SerializeField] private TMP_Text _bestScoreText;
        [SerializeField] private GameObject _newBestBadge;

        [Header("Buttons")]
        [SerializeField] private PushButton _restartButton;
        [SerializeField] private PushButton _homeButton;
        #endregion


        #region Public Methods
        /// <summary>
        /// Round 결과를 표시에 반영합니다.
        /// </summary>
        public void ShowResult(RoundResult result)
        {
            _currentScoreText.text = ScoreFormatter.Format(result.Score);
            _bestScoreText.text = ScoreFormatter.Format(result.BestScore);
            _newBestBadge.SetActive(result.IsNewBest);

            // 낙하한 것이 Egg가 아니면 BrokenEgg가 없으므로 아이콘을 숨김
            _brokenEggIcon.enabled = result.BrokenEgg != null;

            if (result.BrokenEgg != null)
            {
                _brokenEggIcon.sprite = result.BrokenEgg.BaseSprite;
            }
        }

        /// <summary>
        /// 패널의 표시 여부를 설정합니다.
        /// </summary>
        public void SetPanelActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
        #endregion
    }
}
