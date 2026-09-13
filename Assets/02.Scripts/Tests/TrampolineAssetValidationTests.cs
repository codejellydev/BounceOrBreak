using CodeJelly.BounceOrBreak.Game.Trampoline;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// Trampoline 관련 에셋 데이터의 유효성을 검증하는 테스트 클래스.
    /// </summary>
    public class TrampolineAssetValidationTests
    {
        #region Public Methods
        /// <summary>
        /// TrampolineRegistry의 Entry 목록이 유효합니다.
        /// </summary>
        [Test]
        public void TrampolineRegistry_HasNoInvalidEntries()
        {
            List<TrampolineRegistry> registries = new List<TrampolineRegistry>(EditorAssetLoader.LoadAll<TrampolineRegistry>());

            // 에셋을 못 찾으면 무조건 통과하므로 로드 자체를 검증
            Assert.IsNotEmpty(registries, "No TrampolineRegistry asset found.");

            foreach (TrampolineRegistry registry in registries)
            {
                HashSet<TrampolineType> types = new HashSet<TrampolineType>();

                foreach (TrampolineEntry entry in registry.Entries)
                {
                    Assert.IsTrue(types.Add(entry.Type), $"Duplicate TrampolineType in {registry.name}: {entry.Type}.");

                    // AssetReference는 배선되지 않아도 객체 자체는 존재하므로 RuntimeKey를 확인하는 IsValid를 사용
                    Assert.IsTrue(entry.IsValid, $"Prefab AssetReference is invalid for {entry.Type} in {registry.name}.");
                }
            }
        }

        /// <summary>
        /// 등록된 모든 Trampoline 프리팹이 유효합니다.
        /// </summary>
        [Test]
        public void TrampolinePrefab_HasValidRenderer()
        {
            List<TrampolineRegistry> registries = new List<TrampolineRegistry>(EditorAssetLoader.LoadAll<TrampolineRegistry>());

            Assert.IsNotEmpty(registries, "No TrampolineRegistry asset found.");

            foreach (TrampolineRegistry registry in registries)
            {
                foreach (TrampolineEntry entry in registry.Entries)
                {
                    Assume.That(entry.IsValid, $"Invalid entry for {entry.Type} in {registry.name}. See TrampolineRegistry_HasNoInvalidEntries.");

                    string path = AssetDatabase.GUIDToAssetPath(entry.Prefab.AssetGUID);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    // UnityEngine.Object의 null은 Unity 연산자로만 판별되므로 Assert.IsNotNull 대신 사용
                    Assert.IsTrue(prefab != null, $"Prefab for {entry.Type} could not be loaded. GUID: {entry.Prefab.AssetGUID}, path: '{path}'.");
                    Assert.IsTrue(prefab.TryGetComponent(out TrampolineBase trampoline), $"Prefab for {entry.Type} has no TrampolineBase.");
                    Assert.IsTrue(trampoline.IsValid, $"Prefab for {entry.Type} is not valid. Check its renderer wiring.");
                }
            }
        }

        /// <summary>
        /// 모든 TrampolineConfig에 시각 표현 스프라이트가 지정되어 있습니다.
        /// </summary>
        [Test]
        public void TrampolineConfig_HasVisualSprites()
        {
            List<TrampolineConfig> configs = new List<TrampolineConfig>(EditorAssetLoader.LoadAll<TrampolineConfig>());

            Assert.IsNotEmpty(configs, "No TrampolineConfig asset found.");

            foreach (TrampolineConfig config in configs)
            {
                // 프리팹 배선 검사는 Config 내용을 보지 않으므로, 스프라이트 누락은 여기서 잡음
                Assert.IsTrue(config.MatSprite != null, $"MatSprite is not assigned on {config.name}.");
                Assert.IsTrue(config.MarkerSprite != null, $"MarkerSprite is not assigned on {config.name}.");

                // 등록된 매트가 비어 있으면 GetMatSprite가 null을 돌려주므로 남은 횟수마다 확인
                for (int remaining = 1; remaining <= config.MaxUsageCount; remaining++)
                {
                    Assert.IsTrue(config.GetMatSprite(remaining) != null, $"Mat sprite for {remaining} remaining usage is not assigned on {config.name}.");
                }
            }
        }
        #endregion
    }
}
