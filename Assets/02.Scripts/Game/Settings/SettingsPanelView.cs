using CodeJelly.BounceOrBreak.Game.Settings.Audio;
using CodeJelly.BounceOrBreak.Game.Settings.Graphics;
using CodeJelly.BounceOrBreak.Game.Settings.Language;
using CodeJelly.BounceOrBreak.Game.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Game.Settings
{
    /// <summary>
    /// Settings 패널의 View 클래스.
    /// SettingsPanelPresenter로부터 제어됩니다.
    /// </summary>
    public class SettingsPanelView : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// 패널을 닫는 버튼.
        /// </summary>
        public PushButton CloseButton => _closeButton;

        /// <summary>
        /// 패널 외부(Dim 영역) 클릭으로 패널을 닫는 버튼.
        /// </summary>
        public Button DimBackgroundButton => _dimBackgroundButton;

        public AudioSettingsView AudioView => _audioView;
        public GraphicsSettingsView GraphicsView => _graphicsView;
        public LanguageSettingsView LanguageView => _languageView;

        /// <summary>
        /// View가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _closeButton != null
                            && _dimBackgroundButton != null
                            && _audioView != null
                            && _graphicsView != null
                            && _languageView != null;
        #endregion


        #region Private Variables
        [Header("Buttons")]
        [SerializeField] private PushButton _closeButton;
        [SerializeField] private Button _dimBackgroundButton;

        [Header("Categories")]
        [SerializeField] private AudioSettingsView _audioView;
        [SerializeField] private GraphicsSettingsView _graphicsView;
        [SerializeField] private LanguageSettingsView _languageView;
        #endregion
    }
}