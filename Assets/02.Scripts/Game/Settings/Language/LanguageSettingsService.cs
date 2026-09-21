using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Settings;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace CodeJelly.BounceOrBreak.Game.Settings.Language
{
    /// <summary>
    /// 언어 설정의 보관·적용·저장을 담당하는 클래스.
    /// </summary>
    public class LanguageSettingsService : IDisposable
    {
        #region Injection
        private readonly ILanguageSettingsApplier _applier;
        private readonly ISettingsRepository _repository;

        public LanguageSettingsService(
            ILanguageSettingsApplier applier,
            ISettingsRepository repository)
        {
            _applier = applier;
            _repository = repository;
        }
        #endregion


        #region Properties
        /// <summary>
        /// 현재 언어 설정.
        /// </summary>
        public LanguageSettings Settings => _settings;
        #endregion


        #region Private Variables
        private LanguageSettings _settings = new LanguageSettings();
        private CancellationTokenSource _applyCts;
        #endregion


        #region Public Methods
        /// <summary>
        /// 저장된 설정을 불러와 적용합니다.
        /// 기록이 없거나 불러오기에 실패하면 기본값으로 시작합니다.
        /// </summary>
        public async UniTask LoadAsync()
        {
            var result = await _repository.LoadAsync<LanguageSettings>(GameConstants.Language.Keys.REPOSITORY_KEY);

            if (result.IsSuccess == true)
            {
                _settings = result.Value;
            }
            // NotFound는 최초 실행의 정상 흐름이므로 그 외 실패만 로그
            else if (result.Error != SettingsResultError.NotFound)
            {
                AppLogger.Warning($"[LanguageSettingsService] Failed to load language settings: {result.Error}. Using defaults.");
            }

            try
            {
                await _applier.ApplySettingsAsync(_settings, RefreshApplyToken());
            }
            catch (OperationCanceledException)
            {
                // 더 최신 적용 요청이 들어온 것이므로 무시
            }
        }

        /// <summary>
        /// 로케일을 바꾸고 적용합니다.
        /// </summary>
        public void SetLocale(string localeCode)
        {
            if (_settings.LocaleCode == localeCode)
            {
                return;
            }

            _settings.LocaleCode = localeCode;
            ApplyLocale(localeCode).Forget();
        }

        /// <summary>
        /// 현재 설정을 저장합니다.
        /// </summary>
        public async UniTask SaveAsync()
        {
            var result = await _repository.SaveAsync(GameConstants.Language.Keys.REPOSITORY_KEY, _settings);

            if (result.IsSuccess == false)
            {
                AppLogger.Error($"[LanguageSettingsService] Failed to save language settings: {result.Error}");
            }
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _applyCts?.Cancel();
            _applyCts?.Dispose();
            _applyCts = null;
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 직전 적용을 취소하고 로케일을 적용합니다.
        /// </summary>
        private async UniTaskVoid ApplyLocale(string localeCode)
        {
            try
            {
                await _applier.ApplyLanguageAsync(localeCode, RefreshApplyToken());
            }
            catch (OperationCanceledException)
            {
                // 더 최신 적용 요청이 들어온 것이므로 무시
            }
        }

        /// <summary>
        /// 직전 적용의 CancellationTokenSource를 취소하고 새 토큰을 반환합니다.
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