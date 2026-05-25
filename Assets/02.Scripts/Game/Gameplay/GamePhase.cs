namespace CodeJelly.BounceOrBreak.Game.Gameplay
{
    /// <summary>
    /// Game Scene의 진행 단계를 정의합니다.
    /// </summary>
    public enum GamePhase
    {
        /// <summary>
        /// 로딩/초기화 단계
        /// </summary>
        Loading = 0,

        /// <summary>
        /// 플레이 진행 중
        /// </summary>
        Playing = 1,

        /// <summary>
        /// 게임 오버
        /// </summary>
        GameOver = 2,
    }
}