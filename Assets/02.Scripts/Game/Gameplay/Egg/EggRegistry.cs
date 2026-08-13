using CodeJelly.BounceOrBreak.Core.Logging;
using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// Egg 종류별 데이터 매핑을 관리하는 Registry SO.
    /// </summary>
    [CreateAssetMenu(fileName = "EggRegistry", menuName = "Configs/Game/Egg Registry")]
    public class EggRegistry : ScriptableObject
    {
        #region Properties
        /// <summary>
        /// 등록된 EggTypeData 목록
        /// </summary>
        public IReadOnlyList<EggTypeData> Entries => _entries;
        #endregion


        #region Private Variables
        [Tooltip("타입별 Egg 데이터 목록")]
        [SerializeField] private List<EggTypeData> _entries = new List<EggTypeData>();
        #endregion


        #region Public Methods
        /// <summary>
        /// 지정된 타입에 해당하는 EggTypeData를 반환합니다.
        /// </summary>
        public EggTypeData GetData(EggType type)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Type == type)
                {
                    return _entries[i];
                }
            }

            AppLogger.Warning($"[EggRegistry] Data not found for type: {type}");
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
        /// </summary>
        [ContextMenu("Validate Entries")]
        private void ValidateEntries()
        {
            HashSet<EggType> registeredTypes = new HashSet<EggType>();

            for (int i = 0; i < _entries.Count; i++)
            {
                var data = _entries[i];

                if (data == null)
                {
                    AppLogger.Warning($"[EggRegistry] Null entry at index {i}");
                    continue;
                }

                // 중복 타입 검증
                if (registeredTypes.Add(data.Type) == false)
                {
                    AppLogger.Warning($"[EggRegistry] Duplicate type detected: {data.Type} at index {i}");
                }
            }

            AppLogger.Info($"[EggRegistry] Validation completed. ({_entries.Count} entries)");
        }
        #endregion
    }
}