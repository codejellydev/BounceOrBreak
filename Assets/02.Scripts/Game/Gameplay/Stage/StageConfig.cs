using CodeJelly.BounceOrBreak.Game.Trampoline;
using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Stage
{
    /// <summary>
    /// 스테이지 구성을 정의하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "StageConfig", menuName = "Configs/Game/Stage Config")]
    public class StageConfig : ScriptableObject
    {
        #region Properties
        public IReadOnlyList<TrampolineSpawnData> Trampolines => _trampolines;
        #endregion


        #region Private Variables
        [Tooltip("스테이지 시작 시 스폰할 Trampoline 목록")]
        [SerializeField] private List<TrampolineSpawnData> _trampolines = new List<TrampolineSpawnData>();
        #endregion
    }
}
