using CodeJelly.BounceOrBreak.Core.Pause;
using CodeJelly.BounceOrBreak.Game.Bounce;
using CodeJelly.BounceOrBreak.Game.Reflect;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Character
{
    /// <summary>
    /// Character에 대한 인터페이스.
    /// </summary>
    public interface ICharacter : IBounceable, IReflectable, IPauseable
    {
        /// <summary>
        /// Transform 참조.
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Character의 상태를 초기 상태로 되돌립니다.
        /// </summary>
        void ResetState();

        /// <summary>
        /// 물리적 움직임의 고정 여부를 설정합니다.
        /// </summary>
        void SetMotionLocked(bool isLocked);
    }
}