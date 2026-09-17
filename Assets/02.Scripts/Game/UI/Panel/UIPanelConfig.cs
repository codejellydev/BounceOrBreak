using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CodeJelly.BounceOrBreak.Game.UI.Panel
{
    /// <summary>
    /// UI 패널의 키별 설정을 선언하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "UIPanelConfig", menuName = "Configs/Game/UI Panel Config")]
    public class UIPanelConfig : ScriptableObject
    {
        #region Nested Classes
        /// <summary>
        /// 패널 하나의 선언.
        /// </summary>
        [System.Serializable]
        public class Entry
        {
            #region Properties
            public string Key => _key;
            public AssetReferenceGameObject Prefab => _prefab;
            public UIPanelLifetime Lifetime => _lifetime;

            /// <summary>
            /// 프리팹이 배선돼 있는지 확인합니다.
            /// </summary>
            public bool IsValid => _prefab != null && _prefab.RuntimeKeyIsValid();
            #endregion


            #region Private Variables
            [Tooltip("패널 키")]
            [SerializeField] private string _key;

            [Tooltip("패널 프리팹")]
            [SerializeField] private AssetReferenceGameObject _prefab;

            [Tooltip("수명 정책")]
            [SerializeField] private UIPanelLifetime _lifetime = UIPanelLifetime.Cached;
            #endregion
        }
        #endregion


        #region Properties
        public IReadOnlyList<Entry> Entries => _entries;
        #endregion


        #region Private Variables
        [SerializeField] private List<Entry> _entries = new List<Entry>();
        #endregion


        #region Public Methods
        /// <summary>
        /// 키에 해당하는 선언을 찾습니다.
        /// </summary>
        public bool TryGetEntry(string key, out Entry entry)
        {
            foreach (Entry candidate in _entries)
            {
                if (candidate.Key == key)
                {
                    entry = candidate;

                    return true;
                }
            }

            entry = null;

            return false;
        }
        #endregion
    }
}