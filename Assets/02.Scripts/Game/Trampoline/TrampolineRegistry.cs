using CodeJelly.BounceOrBreak.Core.Logging;
using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// TrampolineType별 프리팹 매핑을 관리하는 Registry SO.
    /// </summary>
    [CreateAssetMenu(fileName = "TrampolineRegistry", menuName = "Configs/Game/Trampoline Registry")]
    public class TrampolineRegistry : ScriptableObject
    {
        #region Properties
        /// <summary>
        /// 등록된 TrampolineEntry 목록
        /// </summary>
        public IReadOnlyList<TrampolineEntry> Entries => _entries;
        #endregion


        #region Private Variables
        [Tooltip("TrampolineType별 Trampoline Prefab 매핑 목록")]
        [SerializeField] private List<TrampolineEntry> _entries = new List<TrampolineEntry>();
        #endregion


        #region Unity Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateEntries();
        }
#endif
        #endregion


        #region Public Methods
        /// <summary>
        /// 지정된 TrampolineType에 해당하는 TrampolineEntry를 반환합니다.
        /// </summary>
        public TrampolineEntry GetEntry(TrampolineType type)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Type == type)
                {
                    return _entries[i];
                }
            }

            AppLogger.Warning($"[TrampolineRegistry] Entry not found for type: {type}");
            return null;
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Entry 목록의 유효성을 검증합니다.
        /// </summary>
        private void ValidateEntries()
        {
            HashSet<TrampolineType> registeredTypes = new HashSet<TrampolineType>();

            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];

                // 중복 TrampolineType 검증
                if (registeredTypes.Add(entry.Type) == false)
                {
                    AppLogger.Warning($"[TrampolineRegistry] Duplicate type detected: {entry.Type} at index {i}");
                }

                // AssetReference 유효성 검증
                if (entry.IsValid == false)
                {
                    AppLogger.Warning($"[TrampolineRegistry] Invalid AssetReference for type: {entry.Type} at index {i}");
                }
            }

            AppLogger.Info($"[TrampolineRegistry] Validation completed. ({_entries.Count} entries)");
        }
        #endregion
    }
}