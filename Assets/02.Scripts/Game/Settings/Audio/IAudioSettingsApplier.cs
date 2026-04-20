namespace CodeJelly.BounceOrBreak.Game.Settings.Audio
{
    /// <summary>
    /// 오디오 설정값을 실제 오디오 시스템에 적용하는 인터페이스.
    /// </summary>
    public interface IAudioSettingsApplier
    {
        /// <summary>
        /// 마스터 볼륨을 적용합니다. (0.0 ~ 1.0)
        /// </summary>
        void ApplyMasterVolume(float normalizedValue);

        /// <summary>
        /// 배경음악 볼륨을 적용합니다. (0.0 ~ 1.0)
        /// </summary>
        void ApplyBgmVolume(float normalizedValue);

        /// <summary>
        /// 효과음 볼륨을 적용합니다. (0.0 ~ 1.0)
        /// </summary>
        void ApplySfxVolume(float normalizedValue);

        /// <summary>
        /// 모든 오디오 설정을 적용합니다.
        /// </summary>
        void ApplySettings(AudioSettings settings);
    }
}