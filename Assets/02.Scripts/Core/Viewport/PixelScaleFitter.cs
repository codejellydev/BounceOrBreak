using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Core.Viewport
{
    /// <summary>
    /// 루트 Canvas의 배율을 정수로 맞추는 클래스.
    /// 도트 1개가 항상 정수 픽셀로 그려지도록 화면 폭에서 배율을 구합니다.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
    public class PixelScaleFitter : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// PixelScaleFitter가 유효한지 확인합니다.
        /// 루트 Canvas에 붙어 있고, CanvasScaler가 Constant Pixel Size이며 Reference PPU가 UI 스프라이트 PPU와 같아야 합니다.
        /// </summary>
        public bool IsValid
        {
            get
            {
                Canvas canvas = GetComponent<Canvas>();
                CanvasScaler scaler = GetComponent<CanvasScaler>();

                return canvas != null
                    && canvas.isRootCanvas == true
                    && scaler != null
                    && scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize
                    && Mathf.Approximately(scaler.referencePixelsPerUnit, AppConstants.UI.PIXELS_PER_UNIT) == true;
            }
        }
        #endregion


        #region Private Variables
        private CanvasScaler _scaler;
        #endregion


        #region Unity Lifecycle
        private void OnEnable()
        {
            InitializeComponents();
            Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            Apply();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            // ExecuteAlways 컴포넌트는 도메인 리로드 뒤 Awake 없이 OnEnable만 다시 불리므로 여기서 참조를 잡음
            _scaler ??= GetComponent<CanvasScaler>();
        }

        /// <summary>
        /// 현재 화면 폭에 맞춰 Canvas 배율을 갱신합니다.
        /// </summary>
        private void Apply()
        {
            // OnEnable 전 호출은 무시
            if (_scaler == null)
            {
                return;
            }

            int scaleFactor = Mathf.Max(AppConstants.UI.MIN_SCALE_FACTOR, Screen.width / AppConstants.UI.DESIGN_WIDTH);

            // 배율을 바꾸면 Canvas 크기가 변해 이 메서드가 다시 불리므로 같은 배율이면 건너뜀
            if (Mathf.Approximately(_scaler.scaleFactor, scaleFactor) == true)
            {
                return;
            }

            _scaler.scaleFactor = scaleFactor;

            LogApplied(scaleFactor);
        }

        /// <summary>
        /// 적용 결과를 기록합니다.
        /// </summary>
        private void LogApplied(int scaleFactor)
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            AppLogger.Debug($"[PixelScaleFitter] Applied on {name}. width={Screen.width}, scaleFactor={scaleFactor}.");
        }
        #endregion
    }
}