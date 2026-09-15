using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Wall
{
    /// <summary>
    /// Wall의 설정을 관리하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "WallConfig", menuName = "Configs/Game/Wall Config")]
    public class WallConfig : ScriptableObject
    {
        #region Properties
        public float ReflectionDamping => _reflectionDamping;
        #endregion


        #region Private Variables
        [Header("Physics Settings")]
        [Tooltip("반사 시 속도 감쇠 비율 (0.0 = 완전 정지, 1.0 = 감쇠 없음)")]
        [Range(0f, 1f)]
        [SerializeField] private float _reflectionDamping = GameConstants.Reflect.DEFAULT_DAMPING;
        #endregion
    }
}
