using UnityEngine;

namespace CodeJelly.BounceOrBreak.Core.Viewport
{
    /// <summary>
    /// 화면비와 무관하게 항상 보여야 하는 영역과 그 보장 조건.
    /// </summary>
    public readonly struct ViewportContract
    {
        #region Constructors
        /// <summary>
        /// 가로 절반과 기준 화면비로 계약을 정의합니다.
        /// </summary>
        /// <param name="halfWidth">영역의 가로 절반</param>
        /// <param name="designAspect">영역의 기준 화면비 (가로 / 세로)</param>
        /// <param name="minAspect">보장할 최소 화면비 (가로 / 세로)</param>
        /// <param name="anchor">세로 여백의 배치 기준</param>
        public ViewportContract(
            float halfWidth,
            float designAspect,
            float minAspect,
            ViewportAnchor anchor)
        {
            HalfWidth = halfWidth;

            // 화면비가 유효하지 않으면 세로가 발산하므로 이 경우 정사각형으로 정의
            HalfHeight = (designAspect > 0f)
                ? halfWidth / designAspect
                : halfWidth;

            Anchor = anchor;
            MinAspect = minAspect;
        }
        #endregion


        #region Properties
        public float HalfWidth { get; }

        public float HalfHeight { get; }

        /// <summary>
        /// 세로 여백이 생겼을 때 영역을 붙일 기준.
        /// </summary>
        public ViewportAnchor Anchor { get; }

        /// <summary>
        /// 영역 전체가 보이는 것을 보장하는 최소 화면비 (가로 / 세로).
        /// 이보다 세로로 긴 화면에서는 화면 밖 요소가 보일 수 있음.
        /// </summary>
        public float MinAspect { get; }
        #endregion


        #region Public Methods
        /// <summary>
        /// 주어진 화면비에서 영역 전체가 보이는 Camera 설정을 구합니다.
        /// </summary>
        /// <param name="aspect">화면비 (가로 / 세로)</param>
        public ViewportFit Resolve(float aspect)
        {
            // 화면비가 유효하지 않은 경우 세로 기준으로만 Camera를 설정
            if (aspect <= 0f)
            {
                return CreateFit(HalfHeight);
            }

            // 세로 기준과 가로 기준 중 큰 쪽을 택해야 영역 전체가 들어옴
            return CreateFit(Mathf.Max(HalfHeight, HalfWidth / aspect));
        }

        /// <summary>
        /// 보장하는 최소 화면비에서의 Camera 설정을 구합니다.
        /// </summary>
        public ViewportFit ResolveMinAspect()
        {
            return Resolve(MinAspect);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// orthographicSize로부터 Anchor에 따른 Camera y좌표를 도출합니다.
        /// </summary>
        private ViewportFit CreateFit(float orthographicSize)
        {
            // Bottom은 영역 하단을 화면 하단에 고정하므로, 늘어난 세로 공간이 모두 위쪽에 배치됨
            float centerY = (Anchor == ViewportAnchor.Bottom)
                ? orthographicSize - HalfHeight
                : 0f;

            return new ViewportFit(orthographicSize, centerY);
        }
        #endregion
    }
}
