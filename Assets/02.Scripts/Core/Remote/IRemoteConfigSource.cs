using Cysharp.Threading.Tasks;
using System.Threading;

namespace CodeJelly.BounceOrBreak.Core.Remote
{
    /// <summary>
    /// 원격 설정의 출처 인터페이스.
    /// </summary>
    public interface IRemoteConfigSource
    {
        /// <summary>
        /// 주어진 주소의 설정을 수신해 T로 역직렬화합니다.
        /// </summary>
        /// <param name="timeoutSeconds">응답 대기 제한 시간 (초)</param>
        UniTask<RemoteResult<T>> FetchAsync<T>(string url, float timeoutSeconds, CancellationToken ct) where T : class;
    }
}