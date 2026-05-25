using CodeJelly.BounceOrBreak.Core.Localization;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace CodeJelly.BounceOrBreak.Game.Settings.Language
{
    /// <summary>
    /// LocalizationManager 기반 <see cref="ILanguageSettingsApplier"/> 구현체.
    /// </summary>
    public class LocalizationApplier : ILanguageSettingsApplier
    {
        #region Injection
        private readonly LocalizationManager _localizationManager;

        public LocalizationApplier(LocalizationManager localizationManager)
        {
            _localizationManager = localizationManager;
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// ILanguageSettingsApplier.ApplyLanguageAsync Implementation
        /// </summary>
        public async UniTask ApplyLanguageAsync(string localeCode, CancellationToken cancellationToken = default)
        {
            await _localizationManager.SetLocaleAsync(localeCode, cancellationToken);
        }

        /// <summary>
        /// ILanguageSettingsApplier.ApplySettingsAsync Implementation
        /// </summary>
        public async UniTask ApplySettingsAsync(LanguageSettings settings, CancellationToken cancellationToken = default)
        {
            if (settings == null)
            {
                return;
            }

            await ApplyLanguageAsync(settings.LocaleCode, cancellationToken);
        }
        #endregion
    }
}