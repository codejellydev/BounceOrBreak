using CodeJelly.BounceOrBreak.Game.Bounce;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Character
{
    /// <summary>
    /// Character에 대한 인터페이스.
    /// </summary>
    public interface ICharacter : IBounceable
    {
        /// <summary>
        /// Transform 참조.
        /// </summary>
        Transform Transform { get; }
    }
}