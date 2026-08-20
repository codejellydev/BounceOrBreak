namespace CodeJelly.BounceOrBreak.Core.Viewport
{
    /// <summary>
    /// 보장 영역이 모두 보이는 Camera 설정 결과.
    /// </summary>
    public readonly struct ViewportFit
    {
        #region Constructors
        internal ViewportFit(float orthographicSize, float centerY)
        {
            OrthographicSize = orthographicSize;
            CenterY = centerY;
        }
        #endregion


        #region Properties
        public float OrthographicSize { get; }

        /// <summary>
        /// 적용할 Camera의 y좌표.
        /// </summary>
        public float CenterY { get; }

        /// <summary>
        /// 화면 상단의 y좌표. 화면 밖에 두어야 하는 요소의 배치 기준.
        /// </summary>
        public float TopY => CenterY + OrthographicSize;
        #endregion
    }
}
