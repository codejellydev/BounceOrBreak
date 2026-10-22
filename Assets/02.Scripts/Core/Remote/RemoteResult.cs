namespace CodeJelly.BounceOrBreak.Core.Remote
{
    /// <summary>
    /// 원격 설정 수신의 결과를 표현하는 구조체.
    /// </summary>
    public readonly struct RemoteResult<T> where T : class
    {
        #region Constructors
        private RemoteResult(bool isSuccess, T value, RemoteResultError error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }
        #endregion


        #region Properties
        /// <summary>
        /// 성공 여부.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 반환 데이터. IsSuccess가 true인 경우에만 유의미한 값을 가집니다.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// 에러 유형. IsSuccess가 false인 경우에만 유의미한 값을 가집니다.
        /// </summary>
        public RemoteResultError Error { get; }
        #endregion


        #region Public Methods
        /// <summary>
        /// 성공 결과를 생성합니다.
        /// </summary>
        public static RemoteResult<T> Success(T value)
        {
            return new RemoteResult<T>(true, value, RemoteResultError.None);
        }

        /// <summary>
        /// 실패 결과를 생성합니다.
        /// </summary>
        public static RemoteResult<T> Failure(RemoteResultError error)
        {
            return new RemoteResult<T>(false, null, error);
        }
        #endregion
    }
}