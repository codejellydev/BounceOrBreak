using CodeJelly.BounceOrBreak.Editor.PixelGrid;
using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.UI;
using NUnit.Framework;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeJelly.BounceOrBreak.Tests.Validation
{
    /// <summary>
    /// CanvasLayer 상수와 Canvas 설정의 정합성을 검증하는 테스트 클래스.
    /// </summary>
    public class CanvasLayerValidationTests
    {
        #region Private Variables
        private const string UI_PREFAB_PATH = "Assets/03.Prefabs/UI";
        private const string SCENE_PATH = "Assets/01.Scenes";
        #endregion


        #region Public Methods
        /// <summary>
        /// 모든 루트 Canvas에 유효한 CanvasLayerSorter가 있습니다.
        /// </summary>
        [Test]
        public void RootCanvas_HasValidSorter()
        {
            int checkedCount = 0;

            ForEachRoot((root, path) =>
            {
                foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
                {
                    if (canvas.isRootCanvas == false)
                    {
                        continue;
                    }

                    Assert.IsTrue(canvas.TryGetComponent(out CanvasLayerSorter sorter), $"{canvas.name} in {path} has no CanvasLayerSorter.");
                    Assert.IsTrue(sorter.IsValid, $"CanvasLayerSorter on {canvas.name} in {path} is not valid. Declare its layer in ORDER.");
                    checkedCount++;
                }
            });

            // Canvas를 하나도 못 찾으면 무조건 통과하므로 검사 횟수를 검증
            Assert.Greater(checkedCount, 0, "No root Canvas found.");
        }

        /// <summary>
        /// 모든 CanvasLayer가 ORDER에 한 번씩 선언되어 있습니다.
        /// </summary>
        [Test]
        public void Order_ContainsEveryLayerOnce()
        {
            CanvasLayer[] order = GameConstants.UI.CanvasLayers.ORDER;

            foreach (CanvasLayer layer in Enum.GetValues(typeof(CanvasLayer)))
            {
                int count = 0;

                foreach (CanvasLayer declared in order)
                {
                    if (declared == layer)
                    {
                        count++;
                    }
                }

                Assert.AreEqual(1, count, $"CanvasLayer '{layer}' must be declared once in ORDER.");
            }
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// UI 프리팹과 씬의 루트마다 visitor를 호출합니다.
        /// </summary>
        private static void ForEachRoot(Action<GameObject, string> visitor)
        {
            PixelGridTargets targets = PixelGridTargets.FromFolders(UI_PREFAB_PATH, SCENE_PATH);

            // 폴더가 비어 있으면 무조건 통과하므로 탐색 자체를 검증
            Assert.IsNotEmpty(targets.PrefabPaths, $"No prefab found in {UI_PREFAB_PATH}.");
            Assert.IsNotEmpty(targets.ScenePaths, $"No scene found in {SCENE_PATH}.");

            foreach (string path in targets.PrefabPaths)
            {
                GameObject root = PrefabUtility.LoadPrefabContents(path);

                try
                {
                    visitor(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            foreach (string path in targets.ScenePaths)
            {
                Scene scene = SceneManager.GetSceneByPath(path);
                bool hasOpenedScene = scene.isLoaded == false;

                if (hasOpenedScene == true)
                {
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                }

                try
                {
                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        visitor(root, path);
                    }
                }
                finally
                {
                    // 이미 열려 있던 씬을 닫으면 편집 중인 변경이 사라지므로, 이 테스트가 연 경우에만 닫음
                    if (hasOpenedScene == true)
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }
        }
        #endregion
    }
}