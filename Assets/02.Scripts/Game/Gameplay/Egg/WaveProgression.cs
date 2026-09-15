using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// Egg 누적 스폰 횟수에 대응하는 Wave와 스폰 간격.
    /// Wave가 하나 이상이어야 유효합니다.
    /// </summary>
    public readonly struct WaveProgression
    {
        #region Constructors
        /// <summary>
        /// Wave 목록과 마지막 Wave 이후의 감쇠 규칙으로 진행을 정의합니다.
        /// </summary>
        /// <param name="waves">순서대로 진행할 Wave 목록</param>
        /// <param name="intervalDecayPerSpawn">마지막 Wave를 넘어선 뒤 Egg 하나마다 줄어드는 스폰 간격 (초)</param>
        /// <param name="minSpawnInterval">스폰 간격의 하한 (초)</param>
        public WaveProgression(
            IReadOnlyList<WaveData> waves,
            float intervalDecayPerSpawn,
            float minSpawnInterval)
        {
            IReadOnlyList<WaveData> normalizedWaves = waves ?? Array.Empty<WaveData>();
            int total = 0;

            foreach (WaveData wave in normalizedWaves)
            {
                total += wave.SpawnCount;
            }

            _waves = normalizedWaves;
            _intervalDecayPerSpawn = intervalDecayPerSpawn;
            _minSpawnInterval = minSpawnInterval;
            TotalSpawnCount = total;
        }
        #endregion


        #region Properties
        /// <summary>
        /// 모든 Wave의 스폰 횟수 합.
        /// </summary>
        public int TotalSpawnCount { get; }

        public int WaveCount => _waves.Count;

        public IReadOnlyList<WaveData> Waves => _waves;
        #endregion


        #region Private Variables
        private readonly IReadOnlyList<WaveData> _waves;
        private readonly float _intervalDecayPerSpawn;
        private readonly float _minSpawnInterval;
        #endregion


        #region Public Methods
        /// <summary>
        /// 누적 스폰 횟수에 해당하는 Wave의 인덱스를 반환합니다.
        /// 마지막 Wave를 넘어선 뒤에는 마지막 Wave가 계속 이어집니다.
        /// </summary>
        /// <param name="spawnedCount">지금까지 스폰한 Egg의 누적 횟수</param>
        /// <returns>Waves 목록의 인덱스</returns>
        public int GetWaveIndex(int spawnedCount)
        {
            int remaining = spawnedCount;

            for (int i = 0; i < _waves.Count; i++)
            {
                if (remaining < _waves[i].SpawnCount)
                {
                    return i;
                }

                remaining -= _waves[i].SpawnCount;
            }

            return _waves.Count - 1;
        }

        /// <summary>
        /// 누적 스폰 횟수에 해당하는 Wave를 반환합니다.
        /// </summary>
        /// <param name="spawnedCount">지금까지 스폰한 Egg의 누적 횟수</param>
        public WaveData GetWave(int spawnedCount)
        {
            return _waves[GetWaveIndex(spawnedCount)];
        }

        /// <summary>
        /// 누적 스폰 횟수에 해당하는 스폰 간격을 반환합니다.
        /// 마지막 Wave를 넘어선 뒤에는 Egg 하나마다 간격이 줄어들며, 하한 아래로는 내려가지 않습니다.
        /// </summary>
        /// <param name="spawnedCount">지금까지 스폰한 Egg의 누적 횟수</param>
        /// <returns>스폰 간격 (초)</returns>
        public float GetSpawnInterval(int spawnedCount)
        {
            WaveData wave = GetWave(spawnedCount);

            int beyondLastWaveCount = spawnedCount - TotalSpawnCount;

            float interval = (0 < beyondLastWaveCount)
                ? wave.SpawnInterval - (_intervalDecayPerSpawn * beyondLastWaveCount)
                : wave.SpawnInterval;

            return Mathf.Max(_minSpawnInterval, interval);
        }
        #endregion
    }
}
