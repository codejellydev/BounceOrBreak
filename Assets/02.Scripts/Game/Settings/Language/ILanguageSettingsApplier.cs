using System.Threading;
using Cysharp.Threading.Tasks;

namespace CodeJelly.BounceOrBreak.Game.Settings.Language
{
    /// <summary>
    /// 언어 설정값을 실제 로컬라이제이션 시스템에 적용하는 인터페이스.
    /// </summary>
    public interface ILanguageSettingsApplier
    {
        /// <summary>
        /// 로케일 코드를 적용합니다.
        /// </summary>
        UniTask ApplyLanguageAsync(string localeCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// 모든 언어 설정을 적용합니다.
        /// </summary>
        UniTask ApplySettingsAsync(LanguageSettings settings, CancellationToken cancellationToken = default);
    }
}