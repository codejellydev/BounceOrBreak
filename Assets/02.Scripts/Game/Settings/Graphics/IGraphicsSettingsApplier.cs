namespace CodeJelly.BounceOrBreak.Game.Settings.Graphics
{
    /// <summary>
    /// 그래픽 설정값을 실제 렌더링 시스템에 적용하는 인터페이스.
    /// </summary>
    public interface IGraphicsSettingsApplier
    {
        /// <summary>
        /// 그래픽 품질 수준을 적용합니다.
        /// </summary>
        void ApplyQualityLevel(int qualityLevel);

        /// <summary>
        /// 목표 프레임레이트를 적용합니다.
        /// </summary>
        void ApplyTargetFrameRate(int targetFrameRate);

        /// <summary>
        /// 모든 그래픽 설정을 적용합니다.
        /// </summary>
        void ApplySettings(GraphicsSettings settings);
    }
}