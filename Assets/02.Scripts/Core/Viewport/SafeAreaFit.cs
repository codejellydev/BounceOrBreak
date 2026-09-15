using UnityEngine;

namespace CodeJelly.BounceOrBreak.Core.Viewport
{
    /// <summary>
    /// Screen.safeArea를 RectTransform의 앵커로 환산한 결과.
    /// </summary>
    public readonly struct SafeAreaFit
    {
        #region Constructors
        internal SafeAreaFit(Vector2 anchorMin, Vector2 anchorMax)
        {
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
        }
        #endregion


        #region Properties
        /// <summary>
        /// 화면 전체를 쓰는 결과.
        /// </summary>
        public static SafeAreaFit Full => new SafeAreaFit(Vector2.zero, Vector2.one);

        public Vector2 AnchorMin { get; }

        public Vector2 AnchorMax { get; }
        #endregion


        #region Public Methods
        /// <summary>
        /// safeArea를 화면 크기로 나눠 앵커 비율을 구합니다.
        /// </summary>
        /// <param name="safeArea">Screen.safeArea 값</param>
        /// <param name="screenSize">Screen.width, Screen.height</param>
        public static SafeAreaFit Resolve(Rect safeArea, Vector2 screenSize)
        {
            // 화면 크기를 아직 모르는 시점에는 전체 화면으로 처리
            if (screenSize.x <= 0f || screenSize.y <= 0f)
            {
                return Full;
            }

            // safeArea가 화면을 벗어나면 앵커가 0~1을 넘으므로 고정
            Vector2 anchorMin = Vector2.Max(Vector2.zero, Vector2.Min(Vector2.one, safeArea.min / screenSize));
            Vector2 anchorMax = Vector2.Max(Vector2.zero, Vector2.Min(Vector2.one, safeArea.max / screenSize));

            return new SafeAreaFit(anchorMin, anchorMax);
        }
        #endregion
    }
}
