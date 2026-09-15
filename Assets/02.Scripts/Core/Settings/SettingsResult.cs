namespace CodeJelly.BounceOrBreak.Core.Settings
{
    /// <summary>
    /// 값을 반환하는 설정 저장소 작업(ex. Load)의 결과를 표현하는 구조체.
    /// </summary>
    public readonly struct SettingsResult<T> where T : class
    {
        #region Constructors
        private SettingsResult(bool isSuccess, T value, SettingsResultError error)
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
        public SettingsResultError Error { get; }
        #endregion


        #region Public Methods
        /// <summary>
        /// 성공 결과를 생성합니다.
        /// </summary>
        public static SettingsResult<T> Success(T value)
        {
            return new SettingsResult<T>(true, value, SettingsResultError.None);
        }

        /// <summary>
        /// 실패 결과를 생성합니다.
        /// </summary>
        public static SettingsResult<T> Failure(SettingsResultError error)
        {
            return new SettingsResult<T>(false, null, error);
        }
        #endregion
    }


    /// <summary>
    /// 값을 반환하지 않는 설정 저장소 작업(ex. Save, Delete)의 결과를 표현하는 구조체.
    /// </summary>
    public readonly struct SettingsResult
    {
        #region Constructors
        private SettingsResult(bool isSuccess, SettingsResultError error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }
        #endregion


        #region Properties
        /// <summary>
        /// 성공 여부.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 에러 유형. IsSuccess가 false인 경우에만 유의미한 값을 가집니다.
        /// </summary>
        public SettingsResultError Error { get; }
        #endregion


        #region Public Methods
        /// <summary>
        /// 성공 결과를 생성합니다.
        /// </summary>
        public static SettingsResult Success()
        {
            return new SettingsResult(true, SettingsResultError.None);
        }

        /// <summary>
        /// 실패 결과를 생성합니다.
        /// </summary>
        public static SettingsResult Failure(SettingsResultError error)
        {
            return new SettingsResult(false, error);
        }
        #endregion
    }
}