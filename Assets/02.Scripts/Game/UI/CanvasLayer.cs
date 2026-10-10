namespace CodeJelly.BounceOrBreak.Game.UI
{
    /// <summary>
    /// 루트 Canvas의 계층을 정의합니다.
    /// 값은 순서와 무관하며, 순서는 <see cref="GameConstants.UI.CanvasLayers.ORDER"/>에 선언합니다.
    /// </summary>
    public enum CanvasLayer
    {
        /// <summary>
        /// 씬의 화면과 패널
        /// </summary>
        Scene = 0,

        /// <summary>
        /// 씬 전환 중에 표시되는 화면
        /// </summary>
        Transition = 1,

        /// <summary>
        /// 화면 전체를 덮는 오버레이
        /// </summary>
        Overlay = 2,
    }
}