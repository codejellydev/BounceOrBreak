using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Settings;
using Cysharp.Threading.Tasks;
using R3;
using System;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Settings.Audio
{
    /// <summary>
    /// 오디오 설정 카테고리의 Presenter 클래스.
    /// </summary>
    public class AudioSettingsPresenter : ISettingsCategoryPresenter, IStartable, IDisposable
    {
        #region Injection
        private readonly AudioSettingsView _view;
        private readonly IAudioSettingsApplier _applier;
        private readonly ISettingsRepository _repository;

        public AudioSettingsPresenter(
            AudioSettingsView view,
            IAudioSettingsApplier applier,
            ISettingsRepository repository)
        {
            _view = view;
            _applier = applier;
            _repository = repository;
        }
        #endregion


        #region Private Variables
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private AudioSettings _settings;
        #endregion


        #region VContainer Lifecycle
        public void Start()
        {
            LoadAndApplySettings().Forget();
            BindView();
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// ISettingsCategoryPresenter.OnPanelOpened Implementation
        /// </summary>
        public void OnPanelOpened()
        {
            if (_settings == null)
            {
                AppLogger.Warning("[AudioSettingsPresenter] OnPanelOpened called before settings are loaded.");
                return;
            }

            _view.UpdateDisplay(_settings);
        }

        /// <summary>
        /// ISettingsCategoryPresenter.OnPanelClosed Implementation
        /// </summary>
        public void OnPanelClosed()
        {
            SaveSettings().Forget();
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _disposables.Dispose();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 저장된 설정을 불러오고 오디오 시스템에 적용합니다.
        /// </summary>
        private async UniTaskVoid LoadAndApplySettings()
        {
            var result = await _repository.LoadAsync<AudioSettings>(GameConstants.Audio.Keys.REPOSITORY_KEY);

            if (result.IsSuccess == true)
            {
                _settings = result.Value;
            }
            else
            {
                if (result.Error != SettingsResultError.NotFound)
                {
                    AppLogger.Warning($"[AudioSettingsPresenter] Failed to load audio settings: {result.Error}. Using defaults.");
                }

                _settings = new AudioSettings();
            }

            _applier.ApplySettings(_settings);
        }

        /// <summary>
        /// 현재 설정을 저장소에 저장합니다.
        /// </summary>
        private async UniTaskVoid SaveSettings()
        {
            if (_settings == null)
            {
                AppLogger.Warning("[AudioSettingsPresenter] SaveSettings called before settings are loaded.");
                return;
            }

            var result = await _repository.SaveAsync(GameConstants.Audio.Keys.REPOSITORY_KEY, _settings);

            if (result.IsSuccess == false)
            {
                AppLogger.Error($"[AudioSettingsPresenter] Failed to save audio settings: {result.Error}");
            }
        }

        /// <summary>
        /// View의 슬라이더 변경 이벤트를 바인딩합니다.
        /// </summary>
        private void BindView()
        {
            _view.MasterVolumeSlider
                 .OnValueChangedAsObservable()
                 .Subscribe(OnMasterVolumeChanged)
                 .AddTo(_disposables);

            _view.BgmVolumeSlider
                 .OnValueChangedAsObservable()
                 .Subscribe(OnBgmVolumeChanged)
                 .AddTo(_disposables);

            _view.SfxVolumeSlider
                 .OnValueChangedAsObservable()
                 .Subscribe(OnSfxVolumeChanged)
                 .AddTo(_disposables);
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (_settings == null)
            {
                return;
            }

            _settings.MasterVolume = value;
            _applier.ApplyMasterVolume(value);
        }

        private void OnBgmVolumeChanged(float value)
        {
            if (_settings == null)
            {
                return;
            }

            _settings.BgmVolume = value;
            _applier.ApplyBgmVolume(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (_settings == null)
            {
                return;
            }

            _settings.SfxVolume = value;
            _applier.ApplySfxVolume(value);
        }
        #endregion
    }
}