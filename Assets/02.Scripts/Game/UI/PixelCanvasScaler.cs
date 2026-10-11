using CodeJelly.BounceOrBreak.Core.Viewport;

namespace CodeJelly.BounceOrBreak.Game.UI
{
    /// <summary>
    /// UI Canvas의 배율을 정수로 맞추는 클래스.
    /// </summary>
    public class PixelCanvasScaler : PixelCanvasScalerBase
    {
        #region Properties
        protected override int DesignWidth => GameConstants.UI.DESIGN_WIDTH;
        protected override int PixelsPerUnit => GameConstants.UI.PIXELS_PER_UNIT;
        #endregion
    }
}