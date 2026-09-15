namespace CodeJelly.BounceOrBreak.Game.Gameplay.Goal
{
    /// <summary>
    /// TargetColor와 불일치하는 Egg의 처리 방식을 정의합니다.
    /// </summary>
    public enum MismatchPolicy
    {
        /// <summary>
        /// 일정 시간 머금은 뒤 내보냄
        /// </summary>
        Eject = 0,

        /// <summary>
        /// 흡수함
        /// </summary>
        Absorb = 1,
    }
}
