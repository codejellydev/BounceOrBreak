using CodeJelly.BounceOrBreak.Game.Bounce;
using CodeJelly.BounceOrBreak.Game.Reflect;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Character
{
    /// <summary>
    /// Character에 대한 인터페이스.
    /// </summary>
    public interface ICharacter : IBounceable, IReflectable
    {
        /// <summary>
        /// Transform 참조.
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// 캐릭터의 상태를 초기 상태로 되돌린다.
        /// </summary>
        void ResetState();
    }
}