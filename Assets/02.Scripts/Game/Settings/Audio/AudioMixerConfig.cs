using UnityEngine;
using UnityEngine.Audio;

namespace CodeJelly.BounceOrBreak.Game.Settings.Audio
{
    /// <summary>
    /// AudioMixer 제어에 필요한 설정을 관리하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioMixerConfig", menuName = "Configs/Settings/Audio Mixer Config")]
    public class AudioMixerConfig : ScriptableObject
    {
        #region Properties
        public AudioMixer AudioMixer => _audioMixer;
        public string MasterVolumeParameter => _masterVolumeParameter;
        public string BgmVolumeParameter => _bgmVolumeParameter;
        public string SfxVolumeParameter => _sfxVolumeParameter;
        #endregion


        #region Private Variables
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer _audioMixer;

        [Header("Exposed Parameters")]
        [Tooltip("AudioMixer에서 Expose된 Master 볼륨 파라미터명")]
        [SerializeField] private string _masterVolumeParameter = "MasterVolume";

        [Tooltip("AudioMixer에서 Expose된 BGM 볼륨 파라미터명")]
        [SerializeField] private string _bgmVolumeParameter = "BgmVolume";

        [Tooltip("AudioMixer에서 Expose된 SFX 볼륨 파라미터명")]
        [SerializeField] private string _sfxVolumeParameter = "SfxVolume";
        #endregion
    }
}