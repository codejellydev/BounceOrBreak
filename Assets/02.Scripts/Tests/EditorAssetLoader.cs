using System.Collections.Generic;
using UnityEditor;

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
        public static IEnumerable<T> LoadAll<T>() where T : UnityEngine.Object
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                yield return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }
        #endregion
    }
}
