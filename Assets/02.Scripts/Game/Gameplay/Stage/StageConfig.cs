using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using CodeJelly.BounceOrBreak.Game.Gameplay.Goal;
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

        public MismatchPolicy MismatchPolicy => _mismatchPolicy;

        public EggSpawnConfig SpawnConfig => _spawnConfig;
        #endregion


        #region Private Variables
        [Tooltip("스테이지 시작 시 스폰할 Trampoline 목록")]
        [SerializeField] private List<TrampolineSpawnData> _trampolines = new List<TrampolineSpawnData>();

        [Tooltip("TargetColor와 불일치하는 Egg의 처리 방식")]
        [SerializeField] private MismatchPolicy _mismatchPolicy = MismatchPolicy.Eject;

        [Tooltip("이 스테이지에서 사용할 Egg 스폰 설정")]
        [SerializeField] private EggSpawnConfig _spawnConfig;
        #endregion
    }
}
