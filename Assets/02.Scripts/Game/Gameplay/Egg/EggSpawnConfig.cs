using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// Egg 스폰 동작을 정의하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "EggSpawnConfig", menuName = "Configs/Game/Egg Spawn Config")]
    public class EggSpawnConfig : ScriptableObject
    {
        #region Properties
        public float SpawnInterval => _spawnInterval;

        public Vector2 SpawnPosition => _spawnPosition;

        public IReadOnlyList<EggColor> SpawnCycle => _spawnCycle;
        #endregion


        #region Private Variables
        [Tooltip("Egg 스폰 간격 (초)")]
        [SerializeField] private float _spawnInterval = 10f;

        [Tooltip("Egg 스폰 위치 (월드 좌표)")]
        [SerializeField] private Vector2 _spawnPosition;

        [Tooltip("순환 스폰할 EggColor 순서")]
        [SerializeField] private List<EggColor> _spawnCycle = new List<EggColor>();
        #endregion
    }
}
