using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.Character;
using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests.Validation
{
    /// <summary>
    /// Egg 관련 에셋 데이터의 유효성을 검증하는 테스트 클래스.
    /// </summary>
    public class EggAssetValidationTests
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
        /// 모든 Wave의 EggType이 EggRegistry에 등록되어 있습니다.
        /// </summary>
        [Test]
        public void EggSpawnConfig_WaveTypesExistInRegistry()
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
                Assert.IsNotEmpty(config.Waves, $"No wave is defined in {config.name}.");

                for (int i = 0; i < config.Waves.Count; i++)
                {
                    WaveData wave = config.Waves[i];

                    Assert.IsTrue(0 < wave.SpawnCount, $"Wave {i} has a non-positive SpawnCount in {config.name}.");
                    Assert.IsNotEmpty(wave.Types, $"Wave {i} has no EggType in {config.name}.");

                    HashSet<EggType> distinctTypes = new HashSet<EggType>(wave.Types);
                    bool repeatsSingleType = 1 < wave.Types.Count && distinctTypes.Count == 1;

                    Assert.IsFalse(repeatsSingleType, $"Wave {i} repeats a single EggType in {config.name}.");

                    // WaveProgression.GetSpawnInterval이 하한으로 보정하므로, 하한보다 낮은 간격은 실제 스폰에 반영되지 않음
                    Assert.IsTrue
                    (
                        config.MinSpawnInterval <= wave.SpawnInterval,
                        $"Wave {i} SpawnInterval is below MinSpawnInterval in {config.name}."
                    );

                    foreach (EggType type in wave.Types)
                    {
                        Assert.IsTrue(registeredTypes.Contains(type), $"Unregistered EggType in {config.name} wave {i}: {type}.");
                    }
                }
            }
        }

        /// <summary>
        /// 등록된 Egg 프리팹의 구성이 유효합니다.
        /// </summary>
        [Test]
        public void EggPrefab_IsValid()
        {
            GameObject prefab = EditorAssetLoader.LoadByAddress<GameObject>(GameConstants.Egg.Keys.PREFAB);

            // UnityEngine.Object의 null은 Unity 연산자로만 판별되므로 Assert.IsNotNull 대신 사용
            Assert.IsTrue(prefab != null, $"No prefab is registered at address '{GameConstants.Egg.Keys.PREFAB}'.");
            Assert.IsTrue(prefab.TryGetComponent(out EggController egg), $"{prefab.name} has no EggController.");
            Assert.IsTrue(egg.IsValid, $"{prefab.name} is not valid. Check its renderer wiring.");

            // 직렬화 배선은 IsValid가, 오브젝트 구성은 이 테스트가 확인
            Assert.IsTrue(prefab.GetComponent<ICharacter>() != null, $"{prefab.name} has no ICharacter component.");
        }
        #endregion
    }
}
