using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Bounce
{
    /// <summary>
    /// 튕김 입력 데이터.
    /// TODO: MemoryPack 직렬화 고려 (성능 최적화 시)
    /// </summary>
    public struct BounceParams
    {
        #region Properties
        /// <summary>
        /// 튕긴 표면의 노말 벡터. (정규화된 벡터)
        /// </summary>
        public Vector2 Normal
        {
            get => _normal;
            set
            {
                if (value == Vector2.zero)
                {
                    AppLogger.Warning($"[BounceParams] Zero direction, using default.");
                    _normal = GameConstants.Bounce.DEFAULT_DIRECTION;

                    return;
                }
                _normal = value.normalized;
            }
        }

        /// <summary>
        /// 튕김 세기.
        /// </summary>
        public float Force { get; set; }

        /// <summary>
        /// 충돌 전 속도.
        /// 반사 방향 계산을 위해 충돌 시점의 정확한 속도가 필요하므로 외부에서 전달받습니다.
        /// </summary>
        public Vector2 VelocityBeforeCollision { get; set; }

        /// <summary>
        /// 노말 방향 가중치 (0 = 완전 반사, 1 = 완전 노말)
        /// </summary>
        public float NormalBias { get; set; }
        #endregion


        #region Private Variables
        private Vector2 _normal;
        #endregion
    }
}