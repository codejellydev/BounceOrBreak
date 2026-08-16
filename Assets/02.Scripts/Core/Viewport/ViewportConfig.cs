using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Core.Viewport
{
    /// <summary>
    /// Viewport의 설정을 관리하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "ViewportConfig", menuName = "Configs/Core/Viewport Config")]
    public class ViewportConfig : ScriptableObject
    {
        #region Properties
        /// <summary>
        /// 현재 설정값에 대응하는 ViewportContract.
        /// </summary>
        public ViewportContract Contract => new ViewportContract(
            halfWidth: _halfWidth,
            designAspect: _designAspect,
            minAspect: _minAspect,
            anchor: _anchor);
        #endregion


        #region Private Variables
        [Header("Design Area")]
        [Tooltip("영역의 가로 절반 (월드 유닛)")]
        [SerializeField] private float _halfWidth;

        [Tooltip("영역의 기준 화면비 (가로 / 세로, 9:16 = 0.5625)")]
        [SerializeField] private float _designAspect = AppConstants.Viewport.DEFAULT_DESIGN_ASPECT;

        [Tooltip("세로 여백이 생겼을 때 영역을 붙일 기준")]
        [SerializeField] private ViewportAnchor _anchor;

        [Header("Support Range")]
        [Tooltip("보장할 최소 화면비 (가로 / 세로, 9:20 = 0.45 / 9:22.5 = 0.4)")]
        [SerializeField] private float _minAspect = AppConstants.Viewport.DEFAULT_MIN_ASPECT;
        #endregion


        #region Unity Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            // 0 이하가 들어오면 계약 계산이 발산하므로 하한 적용
            _halfWidth = Mathf.Max(AppConstants.Viewport.MIN_SETTING_VALUE, _halfWidth);
            _designAspect = Mathf.Max(AppConstants.Viewport.MIN_SETTING_VALUE, _designAspect);
            _minAspect = Mathf.Max(AppConstants.Viewport.MIN_SETTING_VALUE, _minAspect);

            if (_minAspect > _designAspect)
            {
                AppLogger.Warning($"[ViewportConfig] Min aspect is wider than the design aspect in {name}. Screens taller than the design ratio are not guaranteed.");
            }
        }
#endif
        #endregion
    }
}
