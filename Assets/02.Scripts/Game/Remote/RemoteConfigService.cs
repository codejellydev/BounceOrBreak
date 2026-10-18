using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Remote;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace CodeJelly.BounceOrBreak.Game.Remote
{
    /// <summary>
    /// 원격 설정을 수신해 보관하는 클래스.
    /// </summary>
    public class RemoteConfigService
    {
        #region Injection
        private readonly IRemoteConfigSource _source;

        public RemoteConfigService(IRemoteConfigSource source)
        {
            _source = source;
        }
        #endregion


        #region Properties
        /// <summary>
        /// 수신한 원격 설정. 아직 수신하지 않았거나 수신하지 못했으면 null.
        /// </summary>
        public RemoteConfig Config { get; private set; }
        #endregion


        #region Public Methods
        /// <summary>
        /// 원격 설정을 한 번 수신합니다.
        /// </summary>
        public async UniTask FetchAsync(CancellationToken ct = default)
        {
            RemoteResult<RemoteConfig> result = await _source.FetchAsync<RemoteConfig>(GameConstants.Remote.CONFIG_URL, GameConstants.Remote.TIMEOUT_SECONDS, ct);

            if (result.IsSuccess == false)
            {
                AppLogger.Warning($"[RemoteConfigService] Failed to fetch remote config: {result.Error}. Continuing without it.");

                return;
            }

            // 새 데이터가 구버전 클라이언트를 깨뜨리지 않도록 모르는 스키마는 설정 전체를 무시
            if (result.Value.SchemaVersion > GameConstants.Remote.SCHEMA_VERSION)
            {
                AppLogger.Warning($"[RemoteConfigService] Unsupported schema version: {result.Value.SchemaVersion} (supported: {GameConstants.Remote.SCHEMA_VERSION}). Ignoring.");

                return;
            }

            Config = result.Value;
            AppLogger.Info($"[RemoteConfigService] Fetched. Latest version: {Config.LatestVersion}");
        }
        #endregion
    }
}