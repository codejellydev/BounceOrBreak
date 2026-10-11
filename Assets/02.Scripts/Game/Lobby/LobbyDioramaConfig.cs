using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using CodeJelly.BounceOrBreak.Game.Trampoline;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Lobby
{
    /// <summary>
    /// LobbyDiorama의 설정을 관리하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "LobbyDioramaConfig", menuName = "Configs/Game/Lobby Diorama Config")]
    public class LobbyDioramaConfig : ScriptableObject
    {
        #region Properties
        public EggData Egg => _egg;

        public Vector2 EggPosition => _eggPosition;

        public TrampolineSpawnData Trampoline => _trampoline;

        /// <summary>
        /// LobbyDioramaConfig가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _egg != null
            && _trampoline != null
            && _trampoline.Config != null
            && _trampoline.Points != null
            && _trampoline.Points.Length == 2;
        #endregion


        #region Private Variables
        [Tooltip("Trampoline 위에서 튀는 Egg의 데이터")]
        [SerializeField] private EggData _egg;

        [Tooltip("Egg가 떨어지기 시작하는 위치 (월드 좌표)")]
        [SerializeField] private Vector2 _eggPosition;

        [Tooltip("Egg를 받는 Trampoline")]
        [SerializeField] private TrampolineSpawnData _trampoline;
        #endregion
    }
}