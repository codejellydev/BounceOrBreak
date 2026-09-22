using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.Settings.Audio;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// AudioSettingsService의 불러오기·적용·저장을 검증하는 테스트 클래스.
    /// </summary>
    public class AudioSettingsServiceTests
    {
        #region Nested Classes
        /// <summary>
        /// 적용 호출을 기록하는 테스트용 IAudioSettingsApplier 구현체.
        /// </summary>
        private sealed class FakeAudioSettingsApplier : IAudioSettingsApplier
        {
            #region Properties
            public AudioSettings AppliedSettings { get; private set; }
            public float AppliedMasterVolume { get; private set; } = -1f;
            #endregion


            #region Public Methods
            /// <summary>
            /// IAudioSettingsApplier.ApplyMasterVolume Implementation
            /// </summary>
            public void ApplyMasterVolume(float normalizedValue)
            {
                AppliedMasterVolume = normalizedValue;
            }

            /// <summary>
            /// IAudioSettingsApplier.ApplyBgmVolume Implementation
            /// </summary>
            public void ApplyBgmVolume(float normalizedValue)
            { }

            /// <summary>
            /// IAudioSettingsApplier.ApplySfxVolume Implementation
            /// </summary>
            public void ApplySfxVolume(float normalizedValue)
            { }

            /// <summary>
            /// IAudioSettingsApplier.ApplySettings Implementation
            /// </summary>
            public void ApplySettings(AudioSettings settings)
            {
                AppliedSettings = settings;
            }
            #endregion
        }
        #endregion


        #region Private Variables
        private FakeSettingsRepository _repository;
        private FakeAudioSettingsApplier _applier;
        private AudioSettingsService _service;
        #endregion


        #region Public Methods
        [SetUp]
        public void SetUp()
        {
            _repository = new FakeSettingsRepository();
            _applier = new FakeAudioSettingsApplier();
            _service = new AudioSettingsService(_applier, _repository);
        }

        /// <summary>
        /// 저장된 설정이 없으면 기본값을 적용합니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_NoSavedSettings_AppliesDefaults()
        {
            await _service.LoadAsync();

            Assert.AreEqual(GameConstants.Audio.Volume.DEFAULT, _service.Settings.MasterVolume);
            Assert.AreSame(_service.Settings, _applier.AppliedSettings);
        }

        /// <summary>
        /// 저장된 설정이 있으면 불러와 적용합니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_SavedSettingsExist_LoadsAndApplies()
        {
            const float savedVolume = 0.3f;

            await _repository.SaveAsync(GameConstants.Audio.Keys.REPOSITORY_KEY, new AudioSettings { MasterVolume = savedVolume });
            await _service.LoadAsync();

            Assert.AreEqual(savedVolume, _service.Settings.MasterVolume);
            Assert.AreSame(_service.Settings, _applier.AppliedSettings);
        }

        /// <summary>
        /// 범위를 벗어난 볼륨은 0~1로 잘라 적용합니다.
        /// </summary>
        [Test]
        public void SetMasterVolume_OutOfRange_ClampsAndApplies()
        {
            _service.SetMasterVolume(1.5f);

            Assert.AreEqual(1f, _service.Settings.MasterVolume);
            Assert.AreEqual(1f, _applier.AppliedMasterVolume);
        }

        /// <summary>
        /// 저장하면 현재 설정이 저장소에 남습니다.
        /// </summary>
        [Test]
        public async Task SaveAsync_PersistsCurrentSettings()
        {
            const float changedVolume = 0.2f;

            _service.SetBgmVolume(changedVolume);
            await _service.SaveAsync();

            var result = await _repository.LoadAsync<AudioSettings>(GameConstants.Audio.Keys.REPOSITORY_KEY);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(changedVolume, result.Value.BgmVolume);
        }
        #endregion
    }
}