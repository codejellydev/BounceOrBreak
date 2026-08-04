using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Goal
{
    /// <summary>
    /// 계란의 골 도달 데이터.
    /// </summary>
    public struct GoalArrival
    {
        #region Properties
        /// <summary>
        /// 계란이 도달한 GoalPort.
        /// </summary>
        public GoalPort Port { get; set; }

        /// <summary>
        /// 도달한 계란.
        /// </summary>
        public EggController Egg { get; set; }

        /// <summary>
        /// TargetType과의 일치 여부.
        /// </summary>
        public bool IsMatched { get; set; }
        #endregion
    }
}