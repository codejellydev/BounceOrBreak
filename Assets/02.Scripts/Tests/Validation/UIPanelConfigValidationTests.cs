using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.UI.Panel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests.Validation
{
    /// <summary>
    /// UIPanelConfig 에셋의 유효성을 검증하는 테스트 클래스.
    /// </summary>
    public class UIPanelConfigValidationTests
    {
        #region Private Variables
        private const string PANEL_ROOT_PREFAB_PATH = "Assets/03.Prefabs/UI/Common/UIPanelRoot.prefab";
        #endregion


        #region Public Methods
        /// <summary>
        /// 모든 Entry가 유효합니다.
        /// </summary>
        [Test]
        public void Entries_AreValid()
        {
            List<UIPanelConfig> configs = new List<UIPanelConfig>(EditorAssetLoader.LoadAll<UIPanelConfig>());

            // 에셋을 못 찾으면 무조건 통과하므로 로드 자체를 검증
            Assert.IsNotEmpty(configs, "No UIPanelConfig asset found.");

            foreach (UIPanelConfig config in configs)
            {
                HashSet<string> keys = new HashSet<string>();

                foreach (UIPanelConfig.Entry entry in config.Entries)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(entry.Key), $"Empty key in {config.name}.");
                    Assert.IsTrue(keys.Add(entry.Key), $"Duplicate key in {config.name}: {entry.Key}.");
                    Assert.IsTrue(entry.IsValid, $"Prefab AssetReference is invalid for '{entry.Key}' in {config.name}.");
                }
            }
        }

        /// <summary>
        /// GameConstants.UI.Keys의 모든 키가 선언돼 있습니다.
        /// </summary>
        [Test]
        public void Entries_CoverEveryKeyInCode()
        {
            List<UIPanelConfig> configs = new List<UIPanelConfig>(EditorAssetLoader.LoadAll<UIPanelConfig>());

            Assert.IsNotEmpty(configs, "No UIPanelConfig asset found.");

            foreach (UIPanelConfig config in configs)
            {
                foreach (FieldInfo field in typeof(GameConstants.UI.Keys).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    string key = (string)field.GetRawConstantValue();

                    Assert.IsTrue(config.TryGetEntry(key, out _), $"'{key}' is used in code but not declared in {config.name}.");
                }
            }
        }

        /// <summary>
        /// Resident로 선언된 키마다 UIPanelRoot 프리팹에 상주 인스턴스가 있습니다.
        /// </summary>
        [Test]
        public void ResidentEntries_ArePlacedInPanelRootPrefab()
        {
            List<UIPanelConfig> configs = new List<UIPanelConfig>(EditorAssetLoader.LoadAll<UIPanelConfig>());
            GameObject rootPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PANEL_ROOT_PREFAB_PATH);

            Assert.IsNotEmpty(configs, "No UIPanelConfig asset found.");
            Assert.IsNotNull(rootPrefab, $"UIPanelRoot prefab not found at {PANEL_ROOT_PREFAB_PATH}.");

            HashSet<string> residentKeys = new HashSet<string>();

            foreach (UIPanelInstance resident in rootPrefab.GetComponentsInChildren<UIPanelInstance>(true))
            {
                Assert.IsTrue(residentKeys.Add(resident.Key), $"Duplicate resident panel '{resident.Key}' in {PANEL_ROOT_PREFAB_PATH}.");
            }

            foreach (UIPanelConfig config in configs)
            {
                foreach (UIPanelConfig.Entry entry in config.Entries)
                {
                    if (entry.Lifetime != UIPanelLifetime.Resident)
                    {
                        continue;
                    }

                    Assert.IsTrue(residentKeys.Contains(entry.Key), $"'{entry.Key}' is Resident in {config.name} but has no instance in {PANEL_ROOT_PREFAB_PATH}.");
                }
            }
        }
        #endregion
    }
}