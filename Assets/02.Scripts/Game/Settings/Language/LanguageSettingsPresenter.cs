using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Settings;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Settings.Language
{
    /// <summary>
    /// 언어 설정 카테고리의 Presenter 클래스.
    /// </summary>
    public class LanguageSettingsPresenter : ISettingsCategoryPresenter, IStartable, IDisposable
    {
        #region Injection
        private readonly LanguageSettingsView _view;
        private readonly ILanguageSettingsApplier _applier;
        private readonly ISettingsRepository _repository;

        public LanguageSettingsPresenter(
            LanguageSettingsView view,
            ILanguageSettingsApplier applier,
            ISettingsRepository repository)
        {
            _view = view;
            _applier = applier;
            _repository = repository;
        }
        #endregion


        #region Private Variables
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private LanguageSettings _settings;
        private CancellationTokenSource _applyCts;
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
                AppLogger.Warning("[LanguageSettingsPresenter] OnPanelOpened called before settings are loaded.");
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
            _applyCts?.Cancel();
            _applyCts?.Dispose();
            _applyCts = null;

            _disposables.Dispose();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 저장된 설정을 불러오고 로컬라이제이션 시스템에 적용합니다.
        /// </summary>
        private async UniTaskVoid LoadAndApplySettings()
        {
            var result = await _repository.LoadAsync<LanguageSettings>(GameConstants.Language.Keys.REPOSITORY_KEY);

            if (result.IsSuccess == true)
            {
                _settings = result.Value;
            }
            else
            {
                if (result.Error != SettingsResultError.NotFound)
                {
                    AppLogger.Warning($"[LanguageSettingsPresenter] Failed to load language settings: {result.Error}. Using defaults.");
                }

                _settings = new LanguageSettings();
            }

            var token = RefreshApplyToken();

            try
            {
                await _applier.ApplySettingsAsync(_settings, token);
            }
            catch (OperationCanceledException)
            {
                // Presenter가 Dispose되었거나 더 최신 요청이 들어옴. 정상 흐름이므로 무시.
            }
        }

        /// <summary>
        /// 현재 설정을 저장소에 저장합니다.
        /// </summary>
        private async UniTaskVoid SaveSettings()
        {
            if (_settings == null)
            {
                AppLogger.Warning("[LanguageSettingsPresenter] SaveSettings called before settings are loaded.");
                return;
            }

            var result = await _repository.SaveAsync(GameConstants.Language.Keys.REPOSITORY_KEY, _settings);

            if (result.IsSuccess == false)
            {
                AppLogger.Error($"[LanguageSettingsPresenter] Failed to save language settings: {result.Error}");
            }
        }

        /// <summary>
        /// View의 토글 변경 이벤트를 바인딩합니다.
        /// </summary>
        private void BindView()
        {
            _view.EnglishToggle
                 .OnValueChangedAsObservable()
                 .Where(isOn => isOn)
                 .Subscribe(_ => OnLanguageSelected(GameConstants.Language.LocaleCode.ENGLISH))
                 .AddTo(_disposables);
        }

        private void OnLanguageSelected(string localeCode)
        {
            if (_settings == null)
            {
                return;
            }

            if (_settings.LocaleCode == localeCode)
            {
                return;
            }

            _settings.LocaleCode = localeCode;
            ApplyLanguage(localeCode).Forget();
        }

        /// <summary>
        /// 직전 Apply 작업을 취소하고 새 작업을 시작합니다.
        /// </summary>
        private async UniTaskVoid ApplyLanguage(string localeCode)
        {
            var token = RefreshApplyToken();

            try
            {
                await _applier.ApplyLanguageAsync(localeCode, token);
            }
            catch (OperationCanceledException)
            {
                // 더 최신 요청이 들어와 취소됨. 정상 흐름이므로 무시.
            }
        }

        /// <summary>
        /// 직전 CancellationTokenSource를 취소/해제하고 새것을 생성하여 반환합니다.
        /// </summary>
        private CancellationToken RefreshApplyToken()
        {
            _applyCts?.Cancel();
            _applyCts?.Dispose();
            _applyCts = new CancellationTokenSource();
            return _applyCts.Token;
        }
        #endregion
    }
}