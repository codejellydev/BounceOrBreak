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
        public float LineWidth => _lineWidth;
        public Material LineMaterial => _lineMaterial;
        public Color LineColor => _lineColor;

        public float BounceForce => _bounceForce;
        public float CenterBounceMultiplier => _centerBounceMultiplier;
        public float CenterThreshold => _centerThreshold;
        public float NormalBias => _normalBias;

        public int MaxUsageCount => _maxUsageCount;
        public float MaxLifetime => _maxLifetime;
        #endregion


        #region Private Variables
        [Header("Visual Settings")]
        [Tooltip("Trampoline의 너비")]
        [SerializeField] private float _lineWidth = GameConstants.Trampoline.DEFAULT_LINE_WIDTH;

        [Tooltip("Trampoline의 라인 렌더링에 사용할 Material")]
        [SerializeField] private Material _lineMaterial;

        [Tooltip("Trampoline의 색상")]
        [SerializeField] private Color _lineColor = Color.white;


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
