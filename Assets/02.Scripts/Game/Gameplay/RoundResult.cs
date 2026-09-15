using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;

namespace CodeJelly.BounceOrBreak.Game.Gameplay
{
    /// <summary>
    /// Round의 종료 결과 데이터.
    /// </summary>
    public struct RoundResult
    {
        #region Properties
        /// <summary>
        /// 이번 Round의 점수.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 이번 Round를 반영한 최고 점수.
        /// </summary>
        public int BestScore { get; set; }

        /// <summary>
        /// 이번 Round가 최고 점수를 갱신했는지 여부.
        /// </summary>
        public bool IsNewBest { get; set; }

        /// <summary>
        /// Round를 끝낸 Egg의 데이터. 낙하한 것이 Egg가 아니면 null.
        /// </summary>
        public EggData BrokenEgg { get; set; }
        #endregion
    }
}
