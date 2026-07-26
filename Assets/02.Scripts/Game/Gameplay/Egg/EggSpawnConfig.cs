using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// 계란 스폰 동작을 정의하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "EggSpawnConfig", menuName = "Configs/Game/Egg Spawn Config")]
    public class EggSpawnConfig : ScriptableObject
    {
        #region Properties
        public float SpawnInterval => _spawnInterval;

        public Vector2 SpawnPosition => _spawnPosition;
        #endregion


        #region Private Variables
        [Tooltip("계란 스폰 간격 (초)")]
        [SerializeField] private float _spawnInterval = 10f;

        [Tooltip("계란 스폰 위치 (월드 좌표)")]
        [SerializeField] private Vector2 _spawnPosition;
        #endregion
    }
}