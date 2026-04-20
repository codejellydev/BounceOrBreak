using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Settings.Audio
{
    /// <summary>
    /// AudioMixer 기반 <see cref="IAudioSettingsApplier"/> 구현체.
    /// 정규화된 볼륨값(0.0 ~ 1.0)을 로그 스케일 dB로 변환하여 AudioMixer에 적용합니다.
    /// </summary>
    public class AudioMixerApplier : IAudioSettingsApplier
    {
        #region Injection
        private readonly AudioMixerConfig _config;

        public AudioMixerApplier(AudioMixerConfig config)
        {
            _config = config;
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// IAudioSettingsApplier.ApplyMasterVolume Implementation
        /// </summary>
        public void ApplyMasterVolume(float normalizedValue)
        {
            SetMixerVolume(_config.MasterVolumeParameter, normalizedValue);
        }

        /// <summary>
        /// IAudioSettingsApplier.ApplyBgmVolume Implementation
        /// </summary>
        public void ApplyBgmVolume(float normalizedValue)
        {
            SetMixerVolume(_config.BgmVolumeParameter, normalizedValue);
        }

        /// <summary>
        /// IAudioSettingsApplier.ApplySfxVolume Implementation
        /// </summary>
        public void ApplySfxVolume(float normalizedValue)
        {
            SetMixerVolume(_config.SfxVolumeParameter, normalizedValue);
        }

        /// <summary>
        /// IAudioSettingsApplier.ApplySettings Implementation
        /// </summary>
        public void ApplySettings(AudioSettings settings)
        {
            if (settings == null)
            {
                AppLogger.Warning("[AudioMixerApplier] Attempted to apply null AudioSettings.");
                return;
            }

            ApplyMasterVolume(settings.MasterVolume);
            ApplyBgmVolume(settings.BgmVolume);
            ApplySfxVolume(settings.SfxVolume);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 정규화된 볼륨값을 AudioMixer에 설정합니다.
        /// </summary>
        private void SetMixerVolume(string parameterName, float normalizedValue)
        {
            if (_config.AudioMixer == null)
            {
                AppLogger.Error("[AudioMixerApplier] AudioMixer is not assigned in config.");
                return;
            }

            float clamped = Mathf.Clamp01(normalizedValue);
            float decibel = NormalizedToDecibel(clamped);

            if (_config.AudioMixer.SetFloat(parameterName, decibel) == false)
            {
                AppLogger.Warning($"[AudioMixerApplier] Failed to set AudioMixer parameter: {parameterName}. Ensure it is exposed in the AudioMixer.");
            }
        }

        /// <summary>
        /// 정규화된 볼륨값을 로그 스케일 dB로 변환합니다.
        /// </summary>
        private float NormalizedToDecibel(float normalizedValue)
        {
            if (normalizedValue < GameConstants.Audio.Volume.MIN_NORMALIZED_THRESHOLD)
            {
                return GameConstants.Audio.Volume.MUTED_DECIBEL;
            }

            return Mathf.Log10(normalizedValue) * GameConstants.Audio.Volume.DECIBEL_MULTIPLIER;
        }
        #endregion
    }
}