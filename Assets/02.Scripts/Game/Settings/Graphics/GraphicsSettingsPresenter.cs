using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Settings;
using Cysharp.Threading.Tasks;
using R3;
using System;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Settings.Graphics
{
    /// <summary>
    /// 그래픽 설정 카테고리의 Presenter 클래스.
    /// </summary>
    public class GraphicsSettingsPresenter : ISettingsCategoryPresenter, IStartable, IDisposable
    {
        #region Injection
        private readonly GraphicsSettingsView _view;
        private readonly IGraphicsSettingsApplier _applier;
        private readonly ISettingsRepository _repository;

        public GraphicsSettingsPresenter(
            GraphicsSettingsView view,
            IGraphicsSettingsApplier applier,
            ISettingsRepository repository)
        {
            _view = view;
            _applier = applier;
            _repository = repository;
        }
        #endregion


        #region Private Variables
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private GraphicsSettings _settings;
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
                AppLogger.Warning("[GraphicsSettingsPresenter] OnPanelOpened called before settings are loaded.");
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
        /// 저장된 설정을 불러오고 렌더링 시스템에 적용합니다.
        /// </summary>
        private async UniTaskVoid LoadAndApplySettings()
        {
            var result = await _repository.LoadAsync<GraphicsSettings>(GameConstants.Graphics.Keys.REPOSITORY_KEY);

            if (result.IsSuccess == true)
            {
                _settings = result.Value;
            }
            else
            {
                if (result.Error != SettingsResultError.NotFound)
                {
                    AppLogger.Warning($"[GraphicsSettingsPresenter] Failed to load graphics settings: {result.Error}. Using defaults.");
                }

                _settings = new GraphicsSettings();
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
                AppLogger.Warning("[GraphicsSettingsPresenter] SaveSettings called before settings are loaded.");
                return;
            }

            var result = await _repository.SaveAsync(GameConstants.Graphics.Keys.REPOSITORY_KEY, _settings);

            if (result.IsSuccess == false)
            {
                AppLogger.Error($"[GraphicsSettingsPresenter] Failed to save graphics settings: {result.Error}");
            }
        }

        /// <summary>
        /// View의 토글 변경 이벤트를 바인딩합니다.
        /// </summary>
        private void BindView()
        {
            _view.LowQualityToggle
                 .OnValueChangedAsObservable()
                 .Where(isOn => isOn)
                 .Subscribe(_ => OnQualityLevelChanged(GameConstants.Graphics.Quality.LOW))
                 .AddTo(_disposables);

            _view.MediumQualityToggle
                 .OnValueChangedAsObservable()
                 .Where(isOn => isOn)
                 .Subscribe(_ => OnQualityLevelChanged(GameConstants.Graphics.Quality.MEDIUM))
                 .AddTo(_disposables);

            _view.HighQualityToggle
                 .OnValueChangedAsObservable()
                 .Where(isOn => isOn)
                 .Subscribe(_ => OnQualityLevelChanged(GameConstants.Graphics.Quality.HIGH))
                 .AddTo(_disposables);
        }

        private void OnQualityLevelChanged(int qualityLevel)
        {
            if (_settings == null)
            {
                return;
            }

            if (_settings.QualityLevel == qualityLevel)
            {
                return;
            }

            _settings.QualityLevel = qualityLevel;
            _applier.ApplyQualityLevel(qualityLevel);
        }
        #endregion
    }
}