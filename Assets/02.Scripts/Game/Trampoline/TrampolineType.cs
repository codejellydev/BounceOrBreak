namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// Trampoline의 형태를 정의합니다.
    /// </summary>
    public enum TrampolineType
    {
        /// <summary>
        /// 일자형 (시작점-끝점 직선)
        /// </summary>
        Straight = 0,

        /// <summary>
        /// 자유형 (그리는대로 생성)
        /// </summary>
        Freeform = 1,
    }
}