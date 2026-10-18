using Newtonsoft.Json;

namespace CodeJelly.BounceOrBreak.Game.Remote
{
    /// <summary>
    /// 원격 설정 JSON의 데이터 모델.
    /// </summary>
    public class RemoteConfig
    {
        #region Properties
        /// <summary>
        /// 스키마 버전. 이 빌드가 지원하는 값보다 크면 설정 전체를 무시합니다.
        /// </summary>
        [JsonProperty("schemaVersion")]
        public int SchemaVersion { get; private set; }

        /// <summary>
        /// 최신 앱 버전.
        /// </summary>
        [JsonProperty("latestVersion")]
        public string LatestVersion { get; private set; }

        /// <summary>
        /// 공지. JSON에 없으면 null.
        /// </summary>
        [JsonProperty("notice")]
        public RemoteNotice Notice { get; private set; }
        #endregion
    }
}