using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using NUnit.Framework;
using System.Collections.Generic;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// Egg 관련 에셋 데이터의 유효성을 검증하는 테스트 클래스.
    /// </summary>
    public class EggConfigValidationTests
    {
        #region Public Methods
        /// <summary>
        /// EggRegistry의 Entry 목록이 유효합니다.
        /// </summary>
        [Test]
        public void EggRegistry_HasNoInvalidEntries()
        {
            List<EggRegistry> registries = new List<EggRegistry>(EditorAssetLoader.LoadAll<EggRegistry>());

            // 에셋을 못 찾으면 무조건 통과하므로 로드 자체를 검증
            Assert.IsNotEmpty(registries, "No EggRegistry asset found.");

            foreach (EggRegistry registry in registries)
            {
                HashSet<EggType> types = new HashSet<EggType>();

                foreach (EggData data in registry.Entries)
                {
                    // UnityEngine.Object의 null은 Unity 연산자로만 판별되므로 Assert.IsNotNull 대신 사용
                    Assert.IsTrue(data != null, $"Null entry in {registry.name}.");
                    Assert.IsTrue(types.Add(data.Type), $"Duplicate EggType in {registry.name}: {data.Type}.");
                    Assert.IsTrue(data.BaseSprite != null, $"BaseSprite is not assigned on {data.name}.");
                }
            }
        }

        /// <summary>
        /// SpawnCycle의 모든 EggType이 EggRegistry에 등록되어 있습니다.
        /// </summary>
        [Test]
        public void EggSpawnConfig_CycleTypesExistInRegistry()
        {
            HashSet<EggType> registeredTypes = new HashSet<EggType>();

            foreach (EggRegistry registry in EditorAssetLoader.LoadAll<EggRegistry>())
            {
                foreach (EggData data in registry.Entries)
                {
                    Assume.That(data != null, $"Null entry in {registry.name}. See EggRegistry_HasNoInvalidEntries.");
                    registeredTypes.Add(data.Type);
                }
            }

            // EggRegistry를 못 찾으면 모든 EggType이 미등록으로 보이므로 원인을 분리
            Assert.IsNotEmpty(registeredTypes, "No registered EggType found.");

            List<EggSpawnConfig> configs = new List<EggSpawnConfig>(EditorAssetLoader.LoadAll<EggSpawnConfig>());

            // 에셋을 못 찾으면 무조건 통과하므로 로드 자체를 검증
            Assert.IsNotEmpty(configs, "No EggSpawnConfig asset found.");

            foreach (EggSpawnConfig config in configs)
            {
                Assert.IsNotEmpty(config.SpawnCycle, $"SpawnCycle is empty in {config.name}.");

                foreach (EggType type in config.SpawnCycle)
                {
                    Assert.IsTrue(registeredTypes.Contains(type), $"Unregistered EggType in {config.name}: {type}.");
                }
            }
        }
        #endregion
    }
}
