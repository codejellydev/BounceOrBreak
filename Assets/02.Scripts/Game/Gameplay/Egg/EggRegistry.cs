using CodeJelly.BounceOrBreak.Core.Logging;
using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// EggColor별 데이터 매핑을 관리하는 Registry SO.
    /// </summary>
    [CreateAssetMenu(fileName = "EggRegistry", menuName = "Configs/Game/Egg Registry")]
    public class EggRegistry : ScriptableObject
    {
        #region Properties
        /// <summary>
        /// 등록된 EggData 목록
        /// </summary>
        public IReadOnlyList<EggData> Entries => _entries;
        #endregion


        #region Private Variables
        [Tooltip("EggColor별 EggData 목록")]
        [SerializeField] private List<EggData> _entries = new List<EggData>();
        #endregion


        #region Public Methods
        /// <summary>
        /// 지정된 EggColor에 해당하는 EggData를 반환합니다.
        /// </summary>
        public EggData GetData(EggColor color)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Color == color)
                {
                    return _entries[i];
                }
            }

            AppLogger.Warning($"[EggRegistry] Data not found for color: {color}");
            return null;
        }
        #endregion


        #region Unity Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateEntries();
        }
#endif
        #endregion


        #region Private Methods
        /// <summary>
        /// Entry 목록의 유효성을 검증합니다.
        /// TODO: CI 테스트 적용 후 ContextMenu 수동 검증은 제거 예정
        /// </summary>
        [ContextMenu("Validate Entries")]
        private void ValidateEntries()
        {
            HashSet<EggColor> registeredColors = new HashSet<EggColor>();

            for (int i = 0; i < _entries.Count; i++)
            {
                var data = _entries[i];

                if (data == null)
                {
                    AppLogger.Warning($"[EggRegistry] Null entry at index {i}");
                    continue;
                }

                // 중복 EggColor 검증
                if (registeredColors.Add(data.Color) == false)
                {
                    AppLogger.Warning($"[EggRegistry] Duplicate color detected: {data.Color} at index {i}");
                }
            }

            AppLogger.Info($"[EggRegistry] Validation completed. ({_entries.Count} entries)");
        }
        #endregion
    }
}
