namespace CodeJelly.BounceOrBreak.Core.Scene
{
    /// <summary>
    /// ISceneInitializer의 초기화 상태를 정의합니다.
    /// </summary>
    public enum SceneInitializationState
    {
        /// <summary>
        /// 초기화 전
        /// </summary>
        None = 0,

        /// <summary>
        /// 초기화 진행 중
        /// </summary>
        Initializing = 1,

        /// <summary>
        /// 초기화 완료
        /// </summary>
        Completed = 2,

        /// <summary>
        /// 초기화 실패
        /// </summary>
        Failed = 3,
    }
}