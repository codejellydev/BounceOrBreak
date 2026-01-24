using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Reflect
{
    /// <summary>
    /// 반사 입력 데이터.
    /// TODO: MemoryPack 직렬화 고려 (성능 최적화 시)
    /// </summary>
    public struct ReflectParams
    {
        #region Properties
        /// <summary>
        /// 반사 노말 벡터. (정규화된 벡터)
        /// 표면에서 바깥으로 향하는 방향.
        /// </summary>
        public Vector2 Normal
        {
            get => _normal;
            set => _normal = value.normalized;
        }

        /// <summary>
        /// 반사 시 속도 감쇠 비율 (0.0 = 완전 정지, 1.0 = 감쇠 없음).
        /// </summary>
        public float Damping { get; set; }

        /// <summary>
        /// 충돌 전 속도.
        /// 반사 방향 계산을 위해 충돌 시점의 정확한 속도가 필요하므로 외부에서 전달받습니다.
        /// </summary>
        public Vector2 VelocityBeforeCollision { get; set; }
        #endregion


        #region Private Variables
        private Vector2 _normal;
        #endregion
    }
}