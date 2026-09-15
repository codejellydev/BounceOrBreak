namespace CodeJelly.BounceOrBreak.Core.Settings
{
    /// <summary>
    /// 설정 데이터 저장 작업의 오류 유형을 정의합니다.
    /// 구현체에 관계없이 통합된 오류 분류를 제공합니다.
    /// </summary>
    public enum SettingsResultError
    {
        /// <summary>
        /// 실패 없음 (성공 상태).
        /// </summary>
        None = 0,

        /// <summary>
        /// 요청한 키에 해당하는 데이터가 존재하지 않음.
        /// </summary>
        NotFound,

        /// <summary>
        /// 직렬화 또는 역직렬화 과정에서 실패.
        /// </summary>
        SerializationError,

        /// <summary>
        /// 파일 시스템 또는 네트워크 I/O 실패.
        /// </summary>
        IOError,

        /// <summary>
        /// 입력 데이터가 유효하지 않음 (null, 형식 불일치 등).
        /// </summary>
        InvalidData,

        /// <summary>
        /// 분류되지 않은 알 수 없는 오류.
        /// </summary>
        Unknown
    }
}