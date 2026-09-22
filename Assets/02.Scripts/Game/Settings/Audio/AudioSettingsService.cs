using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Settings;
using Cysharp.Threading.Tasks;

namespace CodeJelly.BounceOrBreak.Game.Settings.Audio
{
    /// <summary>
    /// 오디오 설정의 보관·적용·저장을 담당하는 클래스.
    /// </summary>
    public class AudioSettingsService
    {
        #region Injection
        private readonly IAudioSettingsApplier _applier;
        private readonly ISettingsRepository _repository;

        public AudioSettingsService(
            IAudioSettingsApplier applier,
            ISettingsRepository repository)
        {
            _applier = applier;
            _repository = repository;
        }
        #endregion


        #region Properties
        /// <summary>
        /// 현재 오디오 설정.
        /// </summary>
        public AudioSettings Settings => _settings;
        #endregion


        #region Private Variables
        private AudioSettings _settings = new AudioSettings();
        #endregion


        #region Public Methods
        /// <summary>
        /// 저장된 설정을 불러와 적용합니다.
        /// 기록이 없거나 불러오기에 실패하면 기본값으로 시작합니다.
        /// </summary>
        public async UniTask LoadAsync()
        {
            var result = await _repository.LoadAsync<AudioSettings>(GameConstants.Audio.Keys.REPOSITORY_KEY);

            if (result.IsSuccess == true)
            {
                _settings = result.Value;
            }
            // NotFound는 최초 실행의 정상 흐름이므로 그 외 실패만 로그
            else if (result.Error != SettingsResultError.NotFound)
            {
                AppLogger.Warning($"[AudioSettingsService] Failed to load audio settings: {result.Error}. Using defaults.");
            }

            _applier.ApplySettings(_settings);
        }

        /// <summary>
        /// 마스터 볼륨을 바꾸고 적용합니다.
        /// </summary>
        public void SetMasterVolume(float value)
        {
            _settings.MasterVolume = value;
            _applier.ApplyMasterVolume(_settings.MasterVolume);
        }

        /// <summary>
        /// 배경음악 볼륨을 바꾸고 적용합니다.
        /// </summary>
        public void SetBgmVolume(float value)
        {
            _settings.BgmVolume = value;
            _applier.ApplyBgmVolume(_settings.BgmVolume);
        }

        /// <summary>
        /// 효과음 볼륨을 바꾸고 적용합니다.
        /// </summary>
        public void SetSfxVolume(float value)
        {
            _settings.SfxVolume = value;
            _applier.ApplySfxVolume(_settings.SfxVolume);
        }

        /// <summary>
        /// 현재 설정을 저장합니다.
        /// </summary>
        public async UniTask SaveAsync()
        {
            var result = await _repository.SaveAsync(GameConstants.Audio.Keys.REPOSITORY_KEY, _settings);

            if (result.IsSuccess == false)
            {
                AppLogger.Error($"[AudioSettingsService] Failed to save audio settings: {result.Error}");
            }
        }
        #endregion
    }
}