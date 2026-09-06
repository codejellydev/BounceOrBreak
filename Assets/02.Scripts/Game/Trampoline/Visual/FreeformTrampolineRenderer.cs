using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// FreeformTrampoline의 시각 표현을 담당하는 클래스.
    /// TODO: LineRenderer를 임시로 사용하며, Freeform 전용 시각 표현으로 교체 예정.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class FreeformTrampolineRenderer : TrampolineRenderer
    {
        #region Properties
        public override bool CanApply => _lineRenderer != null;
        #endregion


        #region Private Variables
        [SerializeField] private LineRenderer _lineRenderer;
        #endregion


        #region Public Methods
        public override void Apply(Vector2[] points, TrampolineConfig config)
        {
            _lineRenderer.positionCount = points.Length;
            _lineRenderer.startWidth = config.Thickness;
            _lineRenderer.endWidth = config.Thickness;

            for (int i = 0; i < points.Length; i++)
            {
                _lineRenderer.SetPosition(i, points[i]);
            }
        }
        #endregion
    }
}
