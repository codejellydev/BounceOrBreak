using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// Egg의 스폰 설정과 Wave 진행을 정의하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "EggSpawnConfig", menuName = "Configs/Game/Egg Spawn Config")]
    public class EggSpawnConfig : ScriptableObject
    {
        #region Properties
        /// <summary>
        /// 현재 설정값에 대응하는 WaveProgression.
        /// </summary>
        public WaveProgression Progression => new WaveProgression(
            waves: _waves,
            intervalDecayPerSpawn: _intervalDecayPerSpawn,
            minSpawnInterval: _minSpawnInterval);

        public Vector2 SpawnPosition => _spawnPosition;

        public float InitialSpawnDelay => _initialSpawnDelay;

        public IReadOnlyList<WaveData> Waves => _waves;

        public float IntervalDecayPerSpawn => _intervalDecayPerSpawn;

        public float MinSpawnInterval => _minSpawnInterval;
        #endregion


        #region Private Variables
        [Tooltip("Egg 스폰 위치 (월드 좌표)")]
        [SerializeField] private Vector2 _spawnPosition;

        [Tooltip("첫 Egg를 스폰하기까지의 지연 (초). Wave의 스폰 간격과 무관")]
        [SerializeField] private float _initialSpawnDelay = GameConstants.Egg.DEFAULT_INITIAL_SPAWN_DELAY;

        [Tooltip("순서대로 진행할 Wave 목록")]
        [SerializeField] private List<WaveData> _waves = new List<WaveData>();

        [Tooltip("마지막 Wave를 넘어선 뒤 Egg 하나마다 줄어드는 스폰 간격 (초)")]
        [SerializeField] private float _intervalDecayPerSpawn = GameConstants.Egg.DEFAULT_INTERVAL_DECAY_PER_SPAWN;

        [Tooltip("스폰 간격의 하한 (초)")]
        [SerializeField] private float _minSpawnInterval = GameConstants.Egg.DEFAULT_MIN_SPAWN_INTERVAL;
        #endregion
    }
}
