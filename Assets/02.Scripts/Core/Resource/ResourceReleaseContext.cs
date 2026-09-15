namespace CodeJelly.BounceOrBreak.Core.Resource
{
    /// <summary>
    /// 리소스의 호출 Context를 정의합니다.
    /// </summary>
    public enum ResourceReleaseContext
    {
        /// <summary>
        /// 일반적인 Release
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 정리 로직에서의 Release
        /// </summary>
        Cleanup = 1,
    }
}