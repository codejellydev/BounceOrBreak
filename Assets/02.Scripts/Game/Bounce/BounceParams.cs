using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Bounce
{
    /// <summary>
    /// 튕김 입력 데이터.
    /// TODO: MemoryPack 직렬화 고려 (성능 최적화 시)
    /// </summary>
    public struct BounceParams
    {
        /// <summary>
        /// 튕김 방향. (정규화된 벡터)
        /// </summary>
        public Vector2 Direction;

        /// <summary>
        /// 튕김 세기.
        /// </summary>
        public float Force;

        /// <summary>
        /// 충돌 지점. (Optional)
        /// </summary>
        public Vector2? ContactPoint;
    }
}