using CodeJelly.BounceOrBreak.Core.Viewport;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// UI 프리팹의 화면 배치 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class UIPrefabValidationTests
    {
        #region Private Variables
        private const string UI_PATH = "Assets/03.Prefabs/UI";
        #endregion


        #region Public Methods
        /// <summary>
        /// UI 프리팹의 모든 SafeAreaFitter가 유효합니다.
        /// </summary>
        [Test]
        public void SafeAreaFitter_IsValid()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { UI_PATH });

            // 폴더가 비어 있으면 무조건 통과하므로 탐색 자체를 검증
            Assert.IsNotEmpty(guids, $"No prefab found in {UI_PATH}.");

            int checkedCount = 0;

            foreach (string guid in guids)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));

                foreach (SafeAreaFitter fitter in prefab.GetComponentsInChildren<SafeAreaFitter>(true))
                {
                    Assert.IsTrue(fitter.IsValid, $"SafeAreaFitter on {fitter.name} in {prefab.name} is not valid. Offsets must be zero and parents must fill the screen.");
                    checkedCount++;
                }
            }

            // 컴포넌트를 하나도 못 찾으면 무조건 통과하므로 검사 횟수를 검증
            Assert.Greater(checkedCount, 0, "No SafeAreaFitter found in UI prefabs.");
        }
        #endregion
    }
}
