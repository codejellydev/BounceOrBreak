using CodeJelly.BounceOrBreak.Core.Logging;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Threading;
using UnityEngine.Networking;

namespace CodeJelly.BounceOrBreak.Core.Remote
{
    /// <summary>
    /// UnityWebRequest 기반 <see cref="IRemoteConfigSource"/> 구현체.
    /// </summary>
    public class HttpRemoteConfigSource : IRemoteConfigSource
    {
        #region Public Methods
        /// <summary>
        /// IRemoteConfigSource.FetchAsync Implementation
        /// </summary>
        public async UniTask<RemoteResult<T>> FetchAsync<T>(string url, float timeoutSeconds, CancellationToken ct) where T : class
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            // 제한 시간이 지나면 요청도 함께 취소해 늦은 응답이 처리되지 않게 함
            using (CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                try
                {
                    await request.SendWebRequest()
                                 .WithCancellation(timeoutCts.Token)
                                 .Timeout(TimeSpan.FromSeconds(timeoutSeconds), DelayType.Realtime, taskCancellationTokenSource: timeoutCts);
                }
                catch (TimeoutException)
                {
                    AppLogger.Warning($"[HttpRemoteConfigSource] Request timed out after {timeoutSeconds}s: {url}");
                    return RemoteResult<T>.Failure(RemoteResultError.Timeout);
                }
                catch (UnityWebRequestException ex)
                {
                    RemoteResultError error = ex.Result == UnityWebRequest.Result.ProtocolError ? RemoteResultError.HttpError : RemoteResultError.NetworkError;
                    AppLogger.Warning($"[HttpRemoteConfigSource] Request failed ({error}): {ex.ResponseCode} {ex.Error}");
                    return RemoteResult<T>.Failure(error);
                }

                T data;

                try
                {
                    data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
                }
                catch (Exception ex)
                {
                    AppLogger.Exception(ex, $"[HttpRemoteConfigSource] Failed to parse response: {url}");
                    return RemoteResult<T>.Failure(RemoteResultError.SerializationError);
                }

                // 빈 본문이나 "null"은 예외 없이 null이 되므로 실패로 처리
                if (data == null)
                {
                    AppLogger.Warning($"[HttpRemoteConfigSource] Empty response: {url}");
                    return RemoteResult<T>.Failure(RemoteResultError.SerializationError);
                }

                return RemoteResult<T>.Success(data);
            }
        }
        #endregion
    }
}