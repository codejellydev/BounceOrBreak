using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Core.Viewport
{
    /// <summary>
    /// 루트 Canvas의 배율을 정수로 맞추는 클래스.
    /// 도트 1개가 항상 정수 픽셀로 그려지도록 화면 폭에서 배율을 구합니다.
    /// CanvasScaler를 대신하므로 같은 Canvas에 함께 붙이지 않습니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public abstract class PixelCanvasScalerBase : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// PixelCanvasScalerBase가 유효한지 확인합니다.
        /// 루트 Canvas에 붙어 있고, 같은 Canvas에 CanvasScaler가 없으며, 설계 폭과 PPU가 양수여야 합니다.
        /// </summary>
        public bool IsValid
        {
            get
            {
                Canvas canvas = GetComponent<Canvas>();

                return canvas != null
                    && canvas.isRootCanvas == true
                    && TryGetComponent(out CanvasScaler _) == false
                    && HasValidSettings == true;
            }
        }

        /// <summary>
        /// 설계 기준 화면 폭 (도트). 실제 화면 폭 ÷ DesignWidth = Canvas 배율
        /// </summary>
        protected abstract int DesignWidth { get; }

        /// <summary>
        /// UI 스프라이트의 PPU. Canvas의 Reference PPU가 되어 도트 1개가 1유닛이 됩니다.
        /// </summary>
        protected abstract int PixelsPerUnit { get; }

        private bool HasValidSettings => DesignWidth > 0 && PixelsPerUnit > 0;
        #endregion


        #region Private Variables
        private Canvas _canvas;
        #endregion


        #region Unity Lifecycle
        private void OnEnable()
        {
            InitializeComponents();
            Apply();

            Canvas.preWillRenderCanvases += Apply;
        }

        private void OnDisable()
        {
            Canvas.preWillRenderCanvases -= Apply;

            Restore();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            // ExecuteAlways 컴포넌트는 도메인 리로드 뒤 Awake 없이 OnEnable만 다시 불리므로 여기서 참조를 잡음
            _canvas ??= GetComponent<Canvas>();
        }

        /// <summary>
        /// 현재 화면 폭에 맞춰 Canvas 배율을 갱신합니다.
        /// </summary>
        private void Apply()
        {
            // 중첩된 Canvas는 루트 Canvas의 배율을 따름
            if (_canvas.isRootCanvas == false)
            {
                return;
            }

            // 설계 폭이나 PPU가 0 이하이면 적용할 수 없음
            if (HasValidSettings == false)
            {
                return;
            }

            int width = Mathf.FloorToInt(_canvas.renderingDisplaySize.x);
            int scaleFactor = Mathf.Max(AppConstants.UI.MIN_SCALE_FACTOR, width / DesignWidth);

            // Canvas를 그리기 직전마다 불리므로 값이 바뀔 때만 넣음
            if (Mathf.Approximately(_canvas.referencePixelsPerUnit, PixelsPerUnit) == false)
            {
                _canvas.referencePixelsPerUnit = PixelsPerUnit;
            }

            if (Mathf.Approximately(_canvas.scaleFactor, scaleFactor) == true)
            {
                return;
            }

            _canvas.scaleFactor = scaleFactor;

            LogApplied(width, scaleFactor);
        }

        /// <summary>
        /// Canvas의 값을 기본값으로 되돌립니다.
        /// </summary>
        private void Restore()
        {
            _canvas.scaleFactor = AppConstants.UI.DEFAULT_SCALE_FACTOR;
            _canvas.referencePixelsPerUnit = AppConstants.UI.DEFAULT_REFERENCE_PIXELS_PER_UNIT;
        }

        /// <summary>
        /// 적용 결과를 기록합니다.
        /// </summary>
        private void LogApplied(int width, int scaleFactor)
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            AppLogger.Debug($"[{GetType().Name}] Applied on {name}. width={width}, scaleFactor={scaleFactor}.");
        }
        #endregion
    }
}