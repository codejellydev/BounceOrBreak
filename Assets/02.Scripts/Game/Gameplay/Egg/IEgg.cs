using CodeJelly.BounceOrBreak.Core.Pause;
using CodeJelly.BounceOrBreak.Core.Pooling;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// Egg에 대한 인터페이스.
    /// Egg의 정체성과 도메인 동작을 정의합니다. (물리적 상호작용 등 범용 캐릭터 로직 관련은 ICharacter)
    /// </summary>
    public interface IEgg : IPoolable, IPauseable
    {
        /// <summary>
        /// Egg의 타입 데이터
        /// </summary>
        EggData Data { get; }

        /// <summary>
        /// Egg를 지정된 타입 데이터로 초기화하고 활성화합니다.
        /// </summary>
        /// <param name="data">적용할 타입 데이터</param>
        /// <param name="position">배치할 위치</param>
        /// <returns>초기화 성공 시 true</returns>
        bool Initialize(EggData data, Vector2 position);

        /// <summary>
        /// 움직임을 고정하고 지정된 위치에 머무릅니다.
        /// </summary>
        /// <param name="position">머무를 위치</param>
        void Hold(Vector2 position);

        /// <summary>
        /// 고정을 해제하고 지정된 방향으로 내보냅니다.
        /// </summary>
        /// <param name="direction">내보낼 방향</param>
        /// <param name="force">내보내는 힘</param>
        void Eject(Vector2 direction, float force);
    }
}
