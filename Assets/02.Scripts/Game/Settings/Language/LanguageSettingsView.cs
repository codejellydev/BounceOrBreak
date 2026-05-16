using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Game.Settings.Language
{
    /// <summary>
    /// 언어 설정 카테고리의 View 클래스.
    /// </summary>
    public class LanguageSettingsView : MonoBehaviour
    {
        #region Properties
        public Toggle EnglishToggle => _englishToggle;
        #endregion


        #region Private Variables
        [Header("Language Toggles")]
        [SerializeField] private Toggle _englishToggle;
        #endregion


        #region Public Methods
        /// <summary>
        /// 주어진 설정값을 토글 UI에 반영합니다.
        /// </summary>
        public void UpdateDisplay(LanguageSettings settings)
        {
            if (settings == null)
            {
                AppLogger.Warning("[LanguageSettingsView] UpdateDisplay called with null settings.");
                return;
            }

            _englishToggle.SetIsOnWithoutNotify(settings.LocaleCode == GameConstants.Language.LocaleCode.ENGLISH);
        }
        #endregion
    }
}