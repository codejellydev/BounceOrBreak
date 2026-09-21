using CodeJelly.BounceOrBreak.Core.Viewport;
using CodeJelly.BounceOrBreak.Game.Gameplay.Pause;
using CodeJelly.BounceOrBreak.Game.Gameplay.UI;
using CodeJelly.BounceOrBreak.Game.Lobby;
using CodeJelly.BounceOrBreak.Game.Settings;
using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// UI 프리팹의 배치 규칙과 배선을 검증하는 테스트 클래스.
    /// </summary>
    public class UIPrefabValidationTests
    {
        #region Private Variables
        private const string UI_PATH = "Assets/03.Prefabs/UI";
        private const string DIM_BACKGROUND_PATH = "Assets/03.Prefabs/UI/Common/Widgets/DimBackground.prefab";
        #endregion


        #region Public Methods
        /// <summary>
        /// UI 프리팹의 모든 SafeAreaFitter가 유효합니다.
        /// </summary>
        [Test]
        public void SafeAreaFitter_IsValid()
        {
            AssertAllValid<SafeAreaFitter>(fitter => fitter.IsValid, "Offsets must be zero and parents must fill the screen.");
        }

        /// <summary>
        /// UI 프리팹의 모든 DimBackground는 SafeAreaFitter 밖에 있습니다.
        /// </summary>
        [Test]
        public void DimBackground_IsOutsideSafeArea()
        {
            GameObject dimPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DIM_BACKGROUND_PATH);

            Assert.IsNotNull(dimPrefab, $"No prefab found at {DIM_BACKGROUND_PATH}.");

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { UI_PATH });
            int checkedCount = 0;

            foreach (string guid in guids)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));

                foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
                {
                    // 이름을 바꿔도 검사에서 빠지지 않도록 원본 프리팹으로 판별
                    if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(child.gameObject) != dimPrefab)
                    {
                        continue;
                    }

                    Assert.IsNull(child.GetComponentInParent<SafeAreaFitter>(true), $"{child.name} in {prefab.name} is under a SafeAreaFitter. Dim must cover the whole screen.");
                    checkedCount++;
                }
            }

            Assert.Greater(checkedCount, 0, "No DimBackground instance found in UI prefabs.");
        }

        /// <summary>
        /// UI 프리팹의 모든 QuitConfirmView가 유효합니다.
        /// </summary>
        [Test]
        public void QuitConfirmView_IsValid()
        {
            AssertAllValid<QuitConfirmView>(view => view.IsValid, "Check its button wiring.");
        }

        /// <summary>
        /// UI 프리팹의 모든 PausePanelView가 유효합니다.
        /// </summary>
        [Test]
        public void PausePanelView_IsValid()
        {
            AssertAllValid<PausePanelView>(view => view.IsValid, "Check its button and score text wiring.");
        }

        /// <summary>
        /// UI 프리팹의 모든 GameplayHudView가 유효합니다.
        /// </summary>
        [Test]
        public void GameplayHudView_IsValid()
        {
            AssertAllValid<GameplayHudView>(view => view.IsValid, "Check its button and score wiring.");
        }

        /// <summary>
        /// UI 프리팹의 모든 GameOverPanelView가 유효합니다.
        /// </summary>
        [Test]
        public void GameOverPanelView_IsValid()
        {
            AssertAllValid<GameOverPanelView>(view => view.IsValid, "Check its button and score wiring.");
        }

        /// <summary>
        /// UI 프리팹의 모든 SettingsPanelView가 유효합니다.
        /// </summary>
        [Test]
        public void SettingsPanelView_IsValid()
        {
            AssertAllValid<SettingsPanelView>(view => view.IsValid, "Check its button and category view wiring.");
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// UI 프리팹의 모든 TComponent가 조건을 만족하는지 검증합니다.
        /// </summary>
        /// <param name="isValid">컴포넌트별 유효 조건</param>
        /// <param name="hint">실패 원인에 대한 안내</param>
        private static void AssertAllValid<TComponent>(Func<TComponent, bool> isValid, string hint) where TComponent : Component
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { UI_PATH });

            // 폴더가 비어 있으면 무조건 통과하므로 탐색 자체를 검증
            Assert.IsNotEmpty(guids, $"No prefab found in {UI_PATH}.");

            int checkedCount = 0;

            foreach (string guid in guids)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));

                foreach (TComponent component in prefab.GetComponentsInChildren<TComponent>(true))
                {
                    Assert.IsTrue(isValid(component), $"{typeof(TComponent).Name} on {component.name} in {prefab.name} is not valid. {hint}");
                    checkedCount++;
                }
            }

            // 컴포넌트를 하나도 못 찾으면 무조건 통과하므로 검사 횟수를 검증
            Assert.Greater(checkedCount, 0, $"No {typeof(TComponent).Name} found in UI prefabs.");
        }
        #endregion
    }
}