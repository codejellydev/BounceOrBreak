using CodeJelly.BounceOrBreak.Core.Logging;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Localization.Settings;

namespace CodeJelly.BounceOrBreak.Core.Localization
{
    /// <summary>
    /// Unity Localization 패키지를 감싸는 관리 클래스.
    /// </summary>
    public class LocalizationManager : IDisposable
    {
        public LocalizationManager()
        {
            _currentLocaleCode = new ReactiveProperty<string>(string.Empty);
        }


        #region Properties
        /// <summary>
        /// 현재 적용된 로케일 코드 (ex. "en"). 미초기화 시 빈 문자열.
        /// </summary>
        public ReadOnlyReactiveProperty<string> CurrentLocaleCode => _currentLocaleCode.ToReadOnlyReactiveProperty();
        #endregion


        #region Private Variables
        private readonly ReactiveProperty<string> _currentLocaleCode;
        #endregion


        #region Public Methods
        /// <summary>
        /// Localization 시스템을 초기화합니다.
        /// 빌드 내 Fallback 로케일이 최소 1개 이상 보장되어야 성공으로 간주합니다.
        /// </summary>
        public async UniTask<bool> InitializeAsync()
        {
            AppLogger.Info("[LocalizationManager] Initializing localization system...");

            try
            {
                await LocalizationSettings.InitializationOperation.ToUniTask();
            }
            catch (Exception ex)
            {
                AppLogger.Error($"[LocalizationManager] Initialization failed: {ex.GetType().Name} - {ex.Message}");
                return false;
            }

            var locales = LocalizationSettings.AvailableLocales?.Locales;
            int localeCount = locales?.Count ?? 0;

            if (localeCount == 0)
            {
                AppLogger.Error("[LocalizationManager] No fallback locales available in build. At least one locale must be present.");
                return false;
            }

            _currentLocaleCode.Value = LocalizationSettings.SelectedLocale?.Identifier.Code ?? string.Empty;
            AppLogger.Info($"[LocalizationManager] Initialized. Available locales: {localeCount}, current: '{_currentLocaleCode.Value}'");
            return true;
        }

        /// <summary>
        /// 로케일을 변경합니다. 해당 로케일이 사용 가능하지 않으면 false 반환.
        /// TODO: 언어팩 다운로드 기능 도입 시 await DownloadLocaleAsync 추가 및 async 승격.
        /// </summary>
        public UniTask<bool> SetLocaleAsync(string localeCode, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(localeCode) == true)
            {
                AppLogger.Warning("[LocalizationManager] SetLocaleAsync called with null or empty locale code.");
                return UniTask.FromResult(false);
            }

            var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            if (locale == null)
            {
                AppLogger.Warning($"[LocalizationManager] Locale not found: '{localeCode}'.");
                return UniTask.FromResult(false);
            }

            LocalizationSettings.SelectedLocale = locale;
            _currentLocaleCode.Value = localeCode;

            AppLogger.Info($"[LocalizationManager] Locale set: '{localeCode}' ({locale.LocaleName})");
            return UniTask.FromResult(true);
        }

        /// <summary>
        /// 사용 가능한 로케일 코드 목록을 반환합니다.
        /// </summary>
        public IReadOnlyList<string> GetAvailableLocaleCodes()
        {
            var locales = LocalizationSettings.AvailableLocales?.Locales;

            if (locales == null || locales.Count == 0)
            {
                return Array.Empty<string>();
            }

            var codes = new string[locales.Count];
            for (int i = 0; i < locales.Count; i++)
            {
                codes[i] = locales[i].Identifier.Code;
            }

            return codes;
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _currentLocaleCode.Dispose();

            AppLogger.Info("[LocalizationManager] Disposed cleanly.");
        }
        #endregion
    }
}