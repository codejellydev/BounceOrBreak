namespace CodeJelly.BounceOrBreak.Core.Pause
{
    /// <summary>
    /// 정지가 가능한 객체의 인터페이스.
    /// </summary>
    public interface IPauseable
    {
        /// <summary>
        /// 진행을 멈춥니다. 이미 멈춰 있으면 무시합니다.
        /// </summary>
        void Pause();

        /// <summary>
        /// 멈춘 진행을 이어갑니다. 멈춰 있지 않으면 무시합니다.
        /// </summary>
        void Resume();
    }
}