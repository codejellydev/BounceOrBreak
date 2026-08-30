using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// SpawnSelectionPolicy.Balanced를 구현하는 IEggTypeSelector.
    /// 정의된 종류를 한 번씩 소진한 뒤 다시 채웁니다. 같은 종류를 여러 번 정의하면 그 비율만큼 자주 등장합니다.
    /// </summary>
    public class BalancedEggTypeSelector : IEggTypeSelector
    {
        #region Private Variables
        /// <summary>
        /// 아직 뽑히지 않은 EggType 묶음.
        /// </summary>
        private readonly List<EggType> _bag = new List<EggType>();
        #endregion


        #region Public Methods
        /// <summary>
        /// IEggTypeSelector.Select Implementation
        /// </summary>
        public EggType Select(WaveData wave)
        {
            if (_bag.Count == 0)
            {
                _bag.AddRange(wave.Types);
            }

            int index = Random.Range(0, _bag.Count);
            EggType type = _bag[index];

            _bag.RemoveAt(index);

            return type;
        }
        #endregion
    }
}
