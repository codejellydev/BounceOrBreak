using Cysharp.Threading.Tasks;
using System.Threading;

namespace CodeJelly.BounceOrBreak.Core.UI
{
    /// <summary>
    /// 화면 전환 연출에 대한 인터페이스.
    /// </summary>
    public interface ITransitionEffect
    {
        /// <summary>
        /// 화면을 덮습니다.
        /// </summary>
        UniTask CoverAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 화면을 걷어냅니다.
        /// </summary>
        UniTask RevealAsync(CancellationToken cancellationToken = default);
    }
}