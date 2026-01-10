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
        /// 튕김 방향. (정규화된 벡터)
        /// </summary>
        public Vector2 Direction 
        {
            get => _direction;
            set
            {
                if(value == Vector2.zero)
                {
                    AppLogger.Warning($"[BounceParams] Zero direction, using default.");
                    _direction = GameConstants.Bounce.DEFAULT_DIRECTION;

                    return;
                }
                _direction = value.normalized;
            } 
        }

        /// <summary>
        /// 튕김 세기.
        /// </summary>
        public float Force { get; set; }

        /// <summary>
        /// 충돌 지점. (Optional)
        /// </summary>
        public Vector2? ContactPoint { get; set; }
        #endregion


        #region Private Variables
        private Vector2 _direction;
        #endregion
    }
}