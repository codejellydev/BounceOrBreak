using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// AssetDatabase 기반으로 프로젝트의 에셋을 불러오는 클래스.
    /// </summary>
    internal static class EditorAssetLoader
    {
        #region Public Methods
        /// <summary>
        /// 프로젝트의 모든 T 타입 에셋을 불러옵니다.
        /// </summary>
        /// <returns>찾은 모든 에셋. 없으면 null이 아니라 빈 시퀀스를 반환</returns>
        public static IEnumerable<T> LoadAll<T>() where T : UnityEngine.Object
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                yield return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        /// <summary>
        /// Addressables 주소로 등록된 에셋을 불러옵니다.
        /// </summary>
        /// <returns>찾은 에셋. 등록되지 않았거나 타입이 다르면 null</returns>
        public static T LoadByAddress<T>(string address) where T : UnityEngine.Object
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                return null;
            }

            foreach (AddressableAssetGroup group in settings.groups)
            {
                if (group == null)
                {
                    continue;
                }

                foreach (AddressableAssetEntry entry in group.entries)
                {
                    if (entry.address == address)
                    {
                        return AssetDatabase.LoadAssetAtPath<T>(entry.AssetPath);
                    }
                }
            }

            return null;
        }
        #endregion
    }
}
