using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// 스폰 진행의 한 Wave 구간을 정의하는 데이터.
    /// </summary>
    [Serializable]
    public class WaveData
    {
        #region Constructors
        /// <summary>
        /// 직렬화를 위한 기본 생성자.
        /// </summary>
        public WaveData()
        { }

        public WaveData(int spawnCount, float spawnInterval, List<EggType> types, SpawnSelectionPolicy selectionPolicy)
        {
            _spawnCount = spawnCount;
            _spawnInterval = spawnInterval;
            _types = types;
            _selectionPolicy = selectionPolicy;
        }
        #endregion


        #region Properties
        public int SpawnCount => _spawnCount;

        public float SpawnInterval => _spawnInterval;

        public IReadOnlyList<EggType> Types => _types;

        public SpawnSelectionPolicy SelectionPolicy => _selectionPolicy;
        #endregion


        #region Private Variables
        [Tooltip("이 Wave의 스폰 횟수")]
        [SerializeField] private int _spawnCount = GameConstants.Egg.DEFAULT_WAVE_SPAWN_COUNT;

        [Tooltip("Egg 스폰 간격 (초)")]
        [SerializeField] private float _spawnInterval = GameConstants.Egg.DEFAULT_WAVE_SPAWN_INTERVAL;

        [Tooltip("이 Wave에서 등장할 수 있는 EggType. 같은 종류를 여러 번 넣으면 그 비율만큼 자주 등장")]
        [SerializeField] private List<EggType> _types = new List<EggType>();

        [Tooltip("다음 EggType을 선택하는 방식")]
        [SerializeField] private SpawnSelectionPolicy _selectionPolicy = SpawnSelectionPolicy.Balanced;
        #endregion
    }
}
