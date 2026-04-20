using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Game.Settings.Audio
{
    /// <summary>
    /// 오디오 설정 카테고리의 View 클래스.
    /// </summary>
    public class AudioSettingsView : MonoBehaviour
    {
        #region Properties
        public Slider MasterVolumeSlider => _masterVolumeSlider;
        public Slider BgmVolumeSlider => _bgmVolumeSlider;
        public Slider SfxVolumeSlider => _sfxVolumeSlider;
        #endregion


        #region Private Variables
        [Header("Volume Sliders")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        #endregion


        #region Public Methods
        /// <summary>
        /// 주어진 설정값을 슬라이더 UI에 반영합니다.
        /// </summary>
        public void UpdateDisplay(AudioSettings settings)
        {
            if (settings == null)
            {
                AppLogger.Warning("[AudioSettingsView] UpdateDisplay called with null settings.");
                return;
            }

            // SetValueWithoutNotify를 사용해 UI 업데이트 시 OnValueChanged 콜백이 트리거되는 것을 방지.
            _masterVolumeSlider.SetValueWithoutNotify(settings.MasterVolume);
            _bgmVolumeSlider.SetValueWithoutNotify(settings.BgmVolume);
            _sfxVolumeSlider.SetValueWithoutNotify(settings.SfxVolume);
        }
        #endregion
    }
}