using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// StraightTrampoline의 시각 표현을 담당하는 클래스.
    /// 매트는 선분 길이만큼, 표식은 중앙 판정 구간만큼 늘려 배치합니다.
    /// </summary>
    public class StraightTrampolineRenderer : TrampolineRenderer
    {
        #region Properties
        public override bool CanApply => _mat != null
                                      && _marker != null
                                      && _mat != _marker
                                      /// 표식이 매트에 가려지지 않아야 하므로 sortingOrder 확인
                                      && _mat.sortingOrder < _marker.sortingOrder;
        #endregion


        #region Private Variables
        /// <summary>
        /// 매트 스프라이트.
        /// </summary>
        [SerializeField] private SpriteRenderer _mat;

        /// <summary>
        /// 중앙 판정 표식 스프라이트.
        /// </summary>
        [SerializeField] private SpriteRenderer _marker;
        #endregion


        #region Public Methods
        public override void Apply(Vector2[] points, TrampolineConfig config)
        {
            // 점의 개수가 2개임은 StraightTrampoline.Initialize가 보장하므로 별도의 검증은 생략
            Vector2 start = points[0];
            Vector2 end = points[1];

            Vector2 center = (start + end) * 0.5f;
            Vector2 direction = end - start;
            float length = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            _mat.size = new Vector2(length, config.Thickness);
            _mat.transform.SetPositionAndRotation(center, rotation);

            _marker.sprite = config.MarkerSprite;
            _marker.size = new Vector2(length * (config.CenterZoneEnd - config.CenterZoneStart), config.Thickness * 0.5f);
            _marker.transform.SetPositionAndRotation(center, rotation);
        }

        public override void SetMatSprite(Sprite sprite)
        {
            _mat.sprite = sprite;
        }
        #endregion
    }
}
