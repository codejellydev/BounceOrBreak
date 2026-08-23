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
        public Button RestartButton => _restartButton;
        #endregion


        #region Private Variables
        [Header("Score")]
        [SerializeField] private TMP_Text _bestScoreText;

        [Header("Buttons")]
        [SerializeField] private Button _restartButton;
        #endregion


        #region Public Methods
        /// <summary>
        /// 최고 점수를 표시에 반영합니다.
        /// </summary>
        public void UpdateBestScore(int score)
        {
            _bestScoreText.text = score.ToString();
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
