using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// Trampoline의 설정을 관리하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "TrampolineConfig", menuName = "Configs/Game/Trampoline Config")]
    public class TrampolineConfig : ScriptableObject
    {
        #region Properties
        public Sprite MatSprite => _matSprite;
        public Sprite MarkerSprite => _markerSprite;
        public float Thickness => _thickness;

        public float BounceForce => _bounceForce;
        public float CenterBounceMultiplier => _centerBounceMultiplier;
        public float NormalBias => _normalBias;

        /// <summary>
        /// 중앙 판정 구간의 시작 비율. 라인 시작점부터의 거리를 전체 길이로 나눈 값 기준.
        /// </summary>
        public float CenterZoneStart => 0.5f - _centerThreshold;

        /// <summary>
        /// 중앙 판정 구간의 끝 비율.
        /// </summary>
        public float CenterZoneEnd => 0.5f + _centerThreshold;

        public int MaxUsageCount => _maxUsageCount;
        public float MaxLifetime => _maxLifetime;
        #endregion


        #region Private Variables
        [Header("Visual Settings")]
        [Tooltip("매트 스프라이트 (Sliced로 늘어남, Border 필수)")]
        [SerializeField] private Sprite _matSprite;

        [Tooltip("중앙 판정 표식 스프라이트")]
        [SerializeField] private Sprite _markerSprite;

        [Tooltip("Trampoline의 두께 (유닛)")]
        [SerializeField] private float _thickness = GameConstants.Trampoline.DEFAULT_THICKNESS;

        [Header("Physics Settings")]
        [Tooltip("Trampoline의 기본 튕김 세기")]
        [SerializeField] private float _bounceForce = GameConstants.Trampoline.DEFAULT_BOUNCE_FORCE;

        [Tooltip("중앙 착지 시 튕김 세기 배수")]
        [SerializeField] private float _centerBounceMultiplier = GameConstants.Trampoline.DEFAULT_CENTER_BOUNCE_MULTIPLIER;

        [Tooltip("중앙 판정 범위 (0 = 정중앙만, 0.5 = 전체 라인)")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _centerThreshold = GameConstants.Trampoline.DEFAULT_CENTER_THRESHOLD;

        [Tooltip("노말 방향 가중치 (0 = 완전 반사, 1 = 완전 노말)")]
        [Range(0f, 1f)]
        [SerializeField] private float _normalBias = GameConstants.Trampoline.DEFAULT_NORMAL_BIAS;

        [Header("Lifecycle Settings")]
        [Tooltip("최대 사용 횟수 (0 = 무제한)")]
        [SerializeField] private int _maxUsageCount = 0;

        [Tooltip("최대 생존 시간 초 단위 (0 = 무제한)")]
        [SerializeField] private float _maxLifetime = 0f;
        #endregion
    }
}
