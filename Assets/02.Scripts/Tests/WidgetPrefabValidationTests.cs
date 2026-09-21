using CodeJelly.BounceOrBreak.Game.UI.Widgets;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// 공용 Widget 프리팹의 배선을 검증하는 테스트 클래스.
    /// </summary>
    public class WidgetPrefabValidationTests
    {
        #region Private Variables
        private const string WIDGETS_PATH = "Assets/03.Prefabs/UI/Common/Widgets";
        #endregion


        #region Public Methods
        /// <summary>
        /// Widget 프리팹의 모든 PushButton이 유효합니다.
        /// </summary>
        [Test]
        public void PushButton_IsValid()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { WIDGETS_PATH });

            // 폴더가 비어 있으면 무조건 통과하므로 탐색 자체를 검증
            Assert.IsNotEmpty(guids, $"No prefab found in {WIDGETS_PATH}.");

            int checkedCount = 0;

            foreach (string guid in guids)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));

                foreach (PushButton button in prefab.GetComponentsInChildren<PushButton>(true))
                {
                    Assert.IsTrue(button.IsValid, $"PushButton on {prefab.name} is not valid. Check its face wiring.");
                    checkedCount++;
                }
            }

            // 컴포넌트를 하나도 못 찾으면 무조건 통과하므로 검사 횟수를 검증
            Assert.Greater(checkedCount, 0, "No PushButton found in widget prefabs.");
        }

        /// <summary>
        /// Widget 프리팹의 모든 ProgressBar가 유효합니다.
        /// </summary>
        [Test]
        public void ProgressBar_IsValid()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { WIDGETS_PATH });

            // 폴더가 비어 있으면 무조건 통과하므로 탐색 자체를 검증
            Assert.IsNotEmpty(guids, $"No prefab found in {WIDGETS_PATH}.");

            int checkedCount = 0;

            foreach (string guid in guids)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));

                foreach (ProgressBar bar in prefab.GetComponentsInChildren<ProgressBar>(true))
                {
                    Assert.IsTrue(bar.IsValid, $"ProgressBar on {prefab.name} is not valid. Check its fill wiring.");
                    checkedCount++;
                }
            }

            // 컴포넌트를 하나도 못 찾으면 무조건 통과하므로 검사 횟수를 검증
            Assert.Greater(checkedCount, 0, "No ProgressBar found in widget prefabs.");
        }
        #endregion
    }
}
