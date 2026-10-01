using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.UI.Widgets
{
    /// <summary>
    /// 진행률을 가로 막대로 표현하는 공용 위젯 클래스.
    /// </summary>
    public class ProgressBar : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// ProgressBar가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _fill != null && Mathf.Approximately(_fill.anchorMin.x, 0f);
        #endregion


        #region Private Variables
        [Tooltip("가로로 늘어나는 Fill. anchorMin.x는 0이어야 함")]
        [SerializeField] private RectTransform _fill;

        private RectTransform _fillArea;
        #endregion


        #region Unity Lifecycle
        private void Awake()
        {
            if (IsValid == false)
            {
                AppLogger.Error($"[ProgressBar] Fill is not assigned or not anchored to the left on {name}.");
                enabled = false;

                return;
            }

            _fillArea = (RectTransform)_fill.parent;

            _fill.offsetMin = new Vector2(0f, _fill.offsetMin.y);
            _fill.offsetMax = new Vector2(0f, _fill.offsetMax.y);

            SetProgress(0f);
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// 진행률을 0~1 값으로 설정합니다.
        /// </summary>
        public void SetProgress(float normalizedValue)
        {
            if (IsValid == false)
            {
                return;
            }

            float width = _fillArea.rect.width;
            float progress = Mathf.Clamp01(normalizedValue);

            // 1유닛 = 1도트이므로 Fill의 끝이 도트 경계에 오도록 폭을 정수 유닛으로 내림
            if (width > 0f)
            {
                progress = Mathf.Floor(progress * width) / width;
            }

            Vector2 anchorMax = _fill.anchorMax;
            anchorMax.x = progress;

            _fill.anchorMax = anchorMax;
        }
        #endregion
    }
}