using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Game.Settings.Graphics
{
    /// <summary>
    /// 그래픽 설정 카테고리의 View 클래스.
    /// </summary>
    public class GraphicsSettingsView : MonoBehaviour
    {
        #region Properties
        public Toggle LowQualityToggle => _lowQualityToggle;
        public Toggle MediumQualityToggle => _mediumQualityToggle;
        public Toggle HighQualityToggle => _highQualityToggle;
        #endregion


        #region Private Variables
        [Header("Quality Level Toggles")]
        [SerializeField] private Toggle _lowQualityToggle;
        [SerializeField] private Toggle _mediumQualityToggle;
        [SerializeField] private Toggle _highQualityToggle;
        #endregion


        #region Public Methods
        /// <summary>
        /// 주어진 설정값을 토글 UI에 반영합니다.
        /// </summary>
        public void UpdateDisplay(GraphicsSettings settings)
        {
            if (settings == null)
            {
                AppLogger.Warning("[GraphicsSettingsView] UpdateDisplay called with null settings.");
                return;
            }

            _lowQualityToggle.SetIsOnWithoutNotify(settings.QualityLevel == GameConstants.Graphics.Quality.LOW);
            _mediumQualityToggle.SetIsOnWithoutNotify(settings.QualityLevel == GameConstants.Graphics.Quality.MEDIUM);
            _highQualityToggle.SetIsOnWithoutNotify(settings.QualityLevel == GameConstants.Graphics.Quality.HIGH);
        }
        #endregion
    }
}