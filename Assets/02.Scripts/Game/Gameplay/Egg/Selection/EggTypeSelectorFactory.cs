using CodeJelly.BounceOrBreak.Core.Logging;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// SpawnSelectionPolicy에 대응하는 IEggTypeSelector를 생성하는 Factory 클래스.
    /// </summary>
    public static class EggTypeSelectorFactory
    {
        #region Public Methods
        /// <summary>
        /// 지정된 정책의 IEggTypeSelector를 생성합니다.
        /// </summary>
        /// <returns>생성된 Selector. 정의되지 않은 정책이면 Balanced</returns>
        public static IEggTypeSelector Create(SpawnSelectionPolicy policy)
        {
            switch (policy)
            {
                case SpawnSelectionPolicy.Balanced:
                    return new BalancedEggTypeSelector();

                case SpawnSelectionPolicy.Random:
                    return new RandomEggTypeSelector();

                default:
                    AppLogger.Error($"[EggTypeSelectorFactory] Unhandled SpawnSelectionPolicy: {policy}.");

                    return new BalancedEggTypeSelector();
            }
        }
        #endregion
    }
}
