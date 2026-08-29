using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// SpawnSelectionPolicy.Random을 구현하는 IEggTypeSelector.
    /// 직전 선택을 고려하지 않으므로 같은 종류가 연속될 수 있습니다.
    /// </summary>
    public class RandomEggTypeSelector : IEggTypeSelector
    {
        #region Public Methods
        /// <summary>
        /// IEggTypeSelector.Select Implementation
        /// </summary>
        public EggType Select(WaveData wave)
        {
            return wave.Types[Random.Range(0, wave.Types.Count)];
        }
        #endregion
    }
}
