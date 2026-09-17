using CodeJelly.BounceOrBreak.Game.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Pause
{
    /// <summary>
    /// 정지 패널의 View 클래스.
    /// PausePanelPresenter로부터 제어됩니다.
    /// </summary>
    public class PausePanelView : MonoBehaviour
    {
        #region Properties
        public PushButton ResumeButton => _resumeButton;
        public PushButton ExitButton => _exitButton;

        /// <summary>
        /// 패널 외부(Dim 영역) 클릭으로 패널을 닫는 버튼.
        /// </summary>
        public Button DimBackgroundButton => _dimBackgroundButton;

        /// <summary>
        /// View가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _resumeButton != null
                            && _exitButton != null
                            && _dimBackgroundButton != null
                            && _currentScoreText != null
                            && _bestScoreText != null;
        #endregion


        #region Private Variables
        [Header("Score")]
        [SerializeField] private TMP_Text _currentScoreText;
        [SerializeField] private TMP_Text _bestScoreText;

        [Header("Buttons")]
        [SerializeField] private PushButton _resumeButton;
        [SerializeField] private PushButton _exitButton;
        [SerializeField] private Button _dimBackgroundButton;
        #endregion


        #region Public Methods
        /// <summary>
        /// 현재 점수를 표시에 반영합니다.
        /// </summary>
        public void SetCurrentScore(int score)
        {
            _currentScoreText.text = score.ToString();
        }

        /// <summary>
        /// 최고 점수를 표시에 반영합니다.
        /// </summary>
        public void SetBestScore(int score)
        {
            _bestScoreText.text = score.ToString();
        }
        #endregion
    }
}