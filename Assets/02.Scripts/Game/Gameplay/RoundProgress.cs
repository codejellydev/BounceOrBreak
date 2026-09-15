using R3;
using System;

namespace CodeJelly.BounceOrBreak.Game.Gameplay
{
    /// <summary>
    /// Round의 진행 상태를 관리하는 클래스.
    /// EggSpawner와 GoalPort가 같은 기준을 통해 진행을 판단합니다.
    /// </summary>
    public class RoundProgress : IDisposable
    {
        #region Properties
        /// <summary>
        /// Round 시작 이후 스폰한 Egg의 누적 횟수.
        /// </summary>
        public ReadOnlyReactiveProperty<int> SpawnedCount => _spawnedCount.ToReadOnlyReactiveProperty();
        #endregion


        #region Private Variables
        private readonly ReactiveProperty<int> _spawnedCount = new ReactiveProperty<int>(0);
        #endregion


        #region Public Methods
        /// <summary>
        /// Egg 하나가 스폰됐음을 진행에 반영합니다.
        /// </summary>
        public void CountSpawn()
        {
            _spawnedCount.Value++;
        }

        /// <summary>
        /// 진행을 Round 시작 상태로 되돌립니다.
        /// </summary>
        public void Reset()
        {
            _spawnedCount.Value = 0;
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _spawnedCount.Dispose();
        }
        #endregion
    }
}
