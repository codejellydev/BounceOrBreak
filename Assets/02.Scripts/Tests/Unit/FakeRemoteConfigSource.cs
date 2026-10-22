using CodeJelly.BounceOrBreak.Core.Remote;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// 테스트용 <see cref="IRemoteConfigSource"/> 구현체.
    /// </summary>
    internal class FakeRemoteConfigSource : IRemoteConfigSource
    {
        #region Properties
        public string Json { get; set; }
        public RemoteResultError Error { get; set; }
        public string LastUrl { get; private set; }
        #endregion


        #region Public Methods
        /// <summary>
        /// IRemoteConfigSource.FetchAsync Implementation
        /// </summary>
        public UniTask<RemoteResult<T>> FetchAsync<T>(string url, float timeoutSeconds, CancellationToken ct) where T : class
        {
            LastUrl = url;

            if (Error != RemoteResultError.None)
            {
                return UniTask.FromResult(RemoteResult<T>.Failure(Error));
            }

            return UniTask.FromResult(RemoteResult<T>.Success(JsonConvert.DeserializeObject<T>(Json)));
        }
        #endregion
    }
}