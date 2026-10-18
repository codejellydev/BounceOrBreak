namespace CodeJelly.BounceOrBreak.Core.Remote
{
    /// <summary>
    /// 원격 설정 수신의 오류 유형을 정의합니다.
    /// </summary>
    public enum RemoteResultError
    {
        /// <summary>
        /// 실패 없음 (성공 상태).
        /// </summary>
        None = 0,

        /// <summary>
        /// 제한 시간 안에 응답을 수신하지 못함.
        /// </summary>
        Timeout,

        /// <summary>
        /// 서버에 연결하지 못함.
        /// </summary>
        NetworkError,

        /// <summary>
        /// 서버가 오류 상태 코드로 응답함.
        /// </summary>
        HttpError,

        /// <summary>
        /// 응답을 역직렬화하는 과정에서 실패.
        /// </summary>
        SerializationError
    }
}