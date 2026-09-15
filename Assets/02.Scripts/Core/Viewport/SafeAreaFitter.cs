using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Core.Viewport
{
    /// <summary>
    /// Screen.safeArea에 맞춰 RectTransform의 앵커를 조정하는 클래스.
    /// Full-Stretched 컨테이너에 붙이며, 자식 UI가 노치·펀치홀·제스처 바 등에 가려지지 않게 합니다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// SafeAreaFitter가 유효한지 확인합니다.
        /// 자신의 오프셋이 0이고, Canvas까지의 모든 부모가 화면 전체를 차지해야 합니다.
        /// </summary>
        public bool IsValid
        {
            get
            {
                RectTransform rectTransform = (RectTransform)transform;

                if (HasZeroOffsets(rectTransform) == false)
                {
                    return false;
                }

                for (RectTransform parent = rectTransform.parent as RectTransform; parent != null; parent = parent.parent as RectTransform)
                {
                    // 루트 Canvas는 항상 화면 전체이므로 검사 대상이 아님
                    if (parent.TryGetComponent(out Canvas canvas) == true && canvas.isRootCanvas == true)
                    {
                        break;
                    }

                    if (IsFullStretch(parent) == false)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        #endregion


        #region Private Variables
        private RectTransform _rectTransform;

        private Rect _appliedSafeArea;
        private Vector2 _appliedScreenSize;
        #endregion


        #region Unity Lifecycle
        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }

        private void OnEnable()
        {
            Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            Apply();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // 앱 밖에서 시스템 UI 설정이 바뀌면 화면 크기는 그대로인 채 safeArea만 달라지므로 복귀 시 재적용
            if (hasFocus == false)
            {
                return;
            }

            Apply();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// rect가 앵커 사각형과 일치하는지 확인합니다.
        /// </summary>
        private static bool HasZeroOffsets(RectTransform rectTransform)
        {
            return rectTransform.anchoredPosition == Vector2.zero && rectTransform.sizeDelta == Vector2.zero;
        }

        /// <summary>
        /// 부모 rect 전체를 차지하는지 확인합니다.
        /// </summary>
        private static bool IsFullStretch(RectTransform rectTransform)
        {
            return rectTransform.anchorMin == Vector2.zero && rectTransform.anchorMax == Vector2.one && HasZeroOffsets(rectTransform);
        }

        /// <summary>
        /// 현재 safeArea에 맞춰 앵커를 갱신합니다.
        /// </summary>
        private void Apply()
        {
            // Awake 전 호출은 무시
            if (_rectTransform == null)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            // 값이 같으면 앵커 대입으로 레이아웃이 다시 dirty되지 않도록 건너뜀
            // 앵커 대입이 OnRectTransformDimensionsChange를 다시 부르므로 이 검사가 재귀도 끊음
            if (safeArea == _appliedSafeArea && screenSize == _appliedScreenSize)
            {
                return;
            }

            SafeAreaFit fit = SafeAreaFit.Resolve(safeArea, screenSize);

            _rectTransform.anchorMin = fit.AnchorMin;
            _rectTransform.anchorMax = fit.AnchorMax;

            _appliedSafeArea = safeArea;
            _appliedScreenSize = screenSize;

            AppLogger.Debug($"[SafeAreaFitter] Applied on {name}. safeArea={safeArea}, screen={screenSize}.");
        }
        #endregion
    }
}
