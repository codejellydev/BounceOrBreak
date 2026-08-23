using System;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// Trampoline을 스폰하는 데 필요한 데이터.
    /// </summary>
    [Serializable]
    public class TrampolineSpawnData
    {
        #region Properties
        public TrampolineType Type => _type;

        public TrampolineConfig Config => _config;

        public Vector2[] Points => _points;
        #endregion


        #region Private Variables
        [Tooltip("스폰할 Trampoline의 형태")]
        [SerializeField] private TrampolineType _type = TrampolineType.Straight;

        [Tooltip("스폰할 Trampoline에 적용할 TrampolineConfig")]
        [SerializeField] private TrampolineConfig _config;

        [Tooltip("LineRenderer의 point 배열 (월드 좌표)")]
        [SerializeField] private Vector2[] _points = new Vector2[0];
        #endregion
    }
}
