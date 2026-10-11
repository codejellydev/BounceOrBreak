using Cysharp.Threading.Tasks;
using System.Threading;

namespace CodeJelly.BounceOrBreak.Core.Scene
{
    /// <summary>
    /// 로딩 화면의 표시를 담당하는 인터페이스.
    /// </summary>
    public interface ILoadingDisplay
    {
        /// <summary>
        /// 로딩이 끝났음을 알리고 표시를 마무리합니다.
        /// </summary>
        UniTask CompleteAsync(CancellationToken ct);
    }
}