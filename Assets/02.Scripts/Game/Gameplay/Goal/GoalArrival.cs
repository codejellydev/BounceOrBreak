using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Goal
{
    /// <summary>
    /// Egg의 Goal 도달 데이터.
    /// </summary>
    public struct GoalArrival
    {
        #region Properties
        /// <summary>
        /// Egg가 도달한 GoalPort.
        /// </summary>
        public GoalPort Port { get; set; }

        /// <summary>
        /// 도달한 Egg.
        /// </summary>
        public IEgg Egg { get; set; }

        /// <summary>
        /// TargetColor와의 일치 여부.
        /// </summary>
        public bool IsMatched { get; set; }
        #endregion
    }
}