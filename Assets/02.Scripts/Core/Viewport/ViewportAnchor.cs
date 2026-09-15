namespace CodeJelly.BounceOrBreak.Core.Viewport
{
    /// <summary>
    /// 화면에 세로 여백이 생겼을 때 영역을 붙일 기준을 정의합니다.
    /// </summary>
    public enum ViewportAnchor
    {
        /// <summary>
        /// 화면 중앙 (여백이 위아래로 분할)
        /// </summary>
        Center = 0,

        /// <summary>
        /// 화면 하단 (여백이 모두 위쪽)
        /// </summary>
        Bottom = 1,
    }
}
