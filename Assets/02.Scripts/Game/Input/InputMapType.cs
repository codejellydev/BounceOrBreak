namespace CodeJelly.BounceOrBreak.Game.Input
{
    /// <summary>
    /// Input Action Map의 Type을 정의.
    /// GameInputAction.inputactions의 Action Map과 1:1로 매칭됩니다.
    /// </summary>
    public enum InputMapType
    {
        /// <summary>
        /// 플레이어 조작 관련 입력
        /// </summary>
        Player,

        /// <summary>
        /// UI 상호작용 관련 입력
        /// </summary>
        UI,

        /// <summary>
        /// 시스템 입력 (뒤로가기, 일시정지 등)
        /// </summary>
        System,
    }
}