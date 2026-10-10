using CodeJelly.BounceOrBreak.Core.Viewport;
using CodeJelly.BounceOrBreak.Editor.PixelGrid;
using CodeJelly.BounceOrBreak.Game;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests.Validation
{
    /// <summary>
    /// UI의 도트 격자 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class PixelGridValidationTests
    {
        #region Private Variables
        private const string UI_PREFAB_PATH = "Assets/03.Prefabs/UI";
        private const string SCENE_PATH = "Assets/01.Scenes";
        private const string UI_SPRITE_PATH = "Assets/09.Images/UI";
        private const string TEXTURE_FILTER = "t:Texture2D";
        #endregion


        #region Public Methods
        /// <summary>
        /// 모든 루트 Canvas에 유효한 PixelCanvasScalerBase가 있습니다.
        /// </summary>
        [Test]
        public void RootCanvas_HasValidScaler()
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

                    Assert.IsTrue(canvas.TryGetComponent(out PixelCanvasScalerBase scaler), $"{canvas.name} in {path} has no PixelCanvasScalerBase. Replace its CanvasScaler.");
                    Assert.IsTrue(scaler.IsValid, $"PixelCanvasScalerBase on {canvas.name} in {path} is not valid. Remove the CanvasScaler next to it.");
                    checkedCount++;
                }
            });

            // Canvas를 하나도 못 찾으면 무조건 통과하므로 검사 횟수를 검증
            Assert.Greater(checkedCount, 0, "No root Canvas found.");
        }

        /// <summary>
        /// UI의 모든 좌표와 크기가 정수입니다.
        /// </summary>
        [Test]
        public void Values_AreWhole()
        {
            AssertNoFinding(PixelGridScanner.FindOffGridValues);
        }

        /// <summary>
        /// 모든 Image가 스프라이트 원본 크기의 정수배입니다.
        /// </summary>
        [Test]
        public void Images_AreNativeMultiples()
        {
            AssertNoFinding(PixelGridScanner.FindNonNativeImages);
        }

        /// <summary>
        /// 모든 UI 스프라이트의 임포트 설정이 픽셀 아트에 맞습니다.
        /// </summary>
        [Test]
        public void UISprites_UsePixelArtImportSettings()
        {
            string[] guids = AssetDatabase.FindAssets(TEXTURE_FILTER, new[] { UI_SPRITE_PATH });

            // 폴더가 비어 있으면 무조건 통과하므로 탐색 자체를 검증
            Assert.IsNotEmpty(guids, $"No texture found in {UI_SPRITE_PATH}.");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                Assert.IsNotNull(importer, $"No TextureImporter found for {path}.");
                Assert.AreEqual(GameConstants.UI.PIXELS_PER_UNIT, importer.spritePixelsPerUnit, $"Pixels Per Unit of {path} does not match.");
                Assert.AreEqual(FilterMode.Point, importer.filterMode, $"Filter Mode of {path} must be Point.");
                Assert.IsFalse(importer.mipmapEnabled, $"Mipmaps of {path} must be off.");
                Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression, $"Compression of {path} must be None.");
            }
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// find가 어느 대상에서도 결과를 반환하지 않는지 검증합니다.
        /// </summary>
        private static void AssertNoFinding(Func<GameObject, List<PixelGridFinding>> find)
        {
            List<string> messages = new List<string>();

            ForEachRoot((root, path) =>
            {
                foreach (PixelGridFinding finding in find(root))
                {
                    messages.Add($"{path} > {finding.Location}: {finding.Detail}");
                }
            });

            Assert.IsEmpty(messages, string.Join(Environment.NewLine, messages));
        }

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
                using (TestScene scene = TestScene.Open(path))
                {
                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        visitor(root, path);
                    }
                }
            }
        }
        #endregion
    }
}