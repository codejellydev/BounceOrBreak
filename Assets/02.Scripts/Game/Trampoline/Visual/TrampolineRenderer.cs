using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// Trampoline의 시각 표현을 담당하는 추상 클래스.
    /// </summary>
    public abstract class TrampolineRenderer : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// 시각 표현을 적용할 준비가 되어 있는지 확인합니다.
        /// </summary>
        public abstract bool CanApply { get; }
        #endregion


        #region Public Methods
        /// <summary>
        /// Trampoline의 시각 표현을 적용합니다.
        /// </summary>
        public abstract void Apply(Vector2[] points, TrampolineConfig config);

        /// <summary>
        /// 매트 스프라이트를 교체합니다.
        /// </summary>
        public abstract void SetMatSprite(Sprite sprite);
        #endregion
    }
}
