namespace CodeJelly.BounceOrBreak.Game.Gameplay.Pause
{
    /// <summary>
    /// 정지 패널의 결과를 정의합니다.
    /// </summary>
    public enum PausePanelResult
    {
        /// <summary>
        /// 이어가기
        /// </summary>
        Resume = 0,

        /// <summary>
        /// 나가기
        /// </summary>
        Exit = 1,

        /// <summary>
        /// 다시 시작
        /// </summary>
        Restart = 2,
    }
}