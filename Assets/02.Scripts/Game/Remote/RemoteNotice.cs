using Newtonsoft.Json;

namespace CodeJelly.BounceOrBreak.Game.Remote
{
    /// <summary>
    /// 원격 설정의 공지 데이터 모델.
    /// </summary>
    public class RemoteNotice
    {
        #region Properties
        /// <summary>
        /// 공지 식별자. 비어 있으면 공지가 없는 것으로 취급합니다.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// 제목.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; private set; }

        /// <summary>
        /// 본문.
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; private set; }

        /// <summary>
        /// 표시 시작 시각 (ISO 8601, UTC). 비어 있으면 즉시.
        /// </summary>
        [JsonProperty("from")]
        public string From { get; private set; }

        /// <summary>
        /// 표시 종료 시각 (ISO 8601, UTC). 비어 있으면 무기한.
        /// </summary>
        [JsonProperty("until")]
        public string Until { get; private set; }
        #endregion
    }
}