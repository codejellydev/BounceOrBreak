namespace CodeJelly.BounceOrBreak.Game.Input
{
    /// <summary>
    /// Input Action Map의 Type을 정의합니다.
    /// GameInputAction.inputactions의 Action Map과 1:1로 매칭됩니다.
    /// </summary>
    [System.Flags]
    public enum InputMapType
    {
        /// <summary>
        /// 아무 Input Map도 활성화하지 않음
        /// </summary>
        None = 0,

        /// <summary>
        /// 플레이어 조작 관련 입력
        /// </summary>
        Player = 1 << 0, // 1

        /// <summary>
        /// UI 상호작용 관련 입력
        /// </summary>
        UI = 1 << 1, // 2

        /// <summary>
        /// 시스템 입력 (뒤로가기, 일시정지 등)
        /// </summary>
        System = 1 << 2, // 4
    }
}