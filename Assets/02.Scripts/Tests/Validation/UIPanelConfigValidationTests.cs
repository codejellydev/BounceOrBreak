using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.UI.Panel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;

namespace CodeJelly.BounceOrBreak.Tests.Validation
{
    /// <summary>
    /// UIPanelConfig 에셋의 유효성을 검증하는 테스트 클래스.
    /// </summary>
    public class UIPanelConfigValidationTests
    {
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
        #endregion
    }
}