using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using NUnit.Framework;
using System.Collections.Generic;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// IEggTypeSelector 구현들의 선택 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class EggTypeSelectorTests
    {
        #region Public Methods
        /// <summary>
        /// Balanced는 한 주기 안에서 Wave의 모든 종류를 한 번씩 선택합니다.
        /// </summary>
        [Test]
        public void Select_BalancedOneCycle_SelectsEveryTypeOnce()
        {
            WaveData wave = CreateWave(EggType.Basic, EggType.Blueberry);
            BalancedEggTypeSelector selector = new BalancedEggTypeSelector();

            List<EggType> selected = SelectMany(selector, wave, wave.Types.Count);

            CollectionAssert.AreEquivalent(wave.Types, selected);
        }

        /// <summary>
        /// Balanced는 주기가 끝나면 다시 채워 같은 분배를 반복합니다.
        /// </summary>
        [Test]
        public void Select_BalancedMultipleCycles_KeepsEachTypeEvenlyDistributed()
        {
            const int cycleCount = 5;

            WaveData wave = CreateWave(EggType.Basic, EggType.Blueberry);
            BalancedEggTypeSelector selector = new BalancedEggTypeSelector();

            List<EggType> selected = SelectMany(selector, wave, wave.Types.Count * cycleCount);

            Assert.AreEqual(cycleCount, CountOf(selected, EggType.Basic));
            Assert.AreEqual(cycleCount, CountOf(selected, EggType.Blueberry));
        }

        /// <summary>
        /// Balanced는 같은 종류가 여러 번 정의되면 그 비율만큼 선택합니다.
        /// </summary>
        [Test]
        public void Select_BalancedDuplicatedType_SelectsByRatio()
        {
            const int cycleCount = 2;
            const int basicPerCycle = 2;
            const int blueberryPerCycle = 1;

            WaveData wave = CreateWave(EggType.Basic, EggType.Basic, EggType.Blueberry);
            BalancedEggTypeSelector selector = new BalancedEggTypeSelector();

            List<EggType> selected = SelectMany(selector, wave, wave.Types.Count * cycleCount);

            Assert.AreEqual(basicPerCycle * cycleCount, CountOf(selected, EggType.Basic));
            Assert.AreEqual(blueberryPerCycle * cycleCount, CountOf(selected, EggType.Blueberry));
        }

        /// <summary>
        /// Balanced는 종류가 하나뿐인 Wave에서도 그 종류만 선택합니다.
        /// </summary>
        [Test]
        public void Select_BalancedSingleType_AlwaysSelectsThatType()
        {
            const int selectCount = 5;

            WaveData wave = CreateWave(EggType.Basic);
            BalancedEggTypeSelector selector = new BalancedEggTypeSelector();

            List<EggType> selected = SelectMany(selector, wave, selectCount);

            Assert.AreEqual(selectCount, CountOf(selected, EggType.Basic));
        }

        /// <summary>
        /// Random은 Wave에 정의된 종류만 선택합니다.
        /// </summary>
        [Test]
        public void Select_Random_SelectsTypeFromWave()
        {
            const int selectCount = 20;

            WaveData wave = CreateWave(EggType.Basic, EggType.Blueberry);
            RandomEggTypeSelector selector = new RandomEggTypeSelector();

            List<EggType> selected = SelectMany(selector, wave, selectCount);
            HashSet<EggType> distinctTypes = new HashSet<EggType>(selected);

            CollectionAssert.IsSubsetOf(distinctTypes, wave.Types);
        }

        /// <summary>
        /// Random은 여러 번 선택하면 정의된 종류가 모두 등장합니다.
        /// </summary>
        [Test]
        public void Select_RandomManyTimes_SelectsEveryType()
        {
            const int selectCount = 20;

            WaveData wave = CreateWave(EggType.Basic, EggType.Blueberry);
            RandomEggTypeSelector selector = new RandomEggTypeSelector();

            List<EggType> selected = SelectMany(selector, wave, selectCount);
            HashSet<EggType> distinctTypes = new HashSet<EggType>(selected);

            // 20회가 모두 한 종류로 몰릴 확률은 약 50만분의 1이므로, 편중된 구현을 사실상 걸러냄
            CollectionAssert.AreEquivalent(wave.Types, distinctTypes);
        }

        /// <summary>
        /// Factory는 정책에 대응하는 Selector를 생성합니다.
        /// </summary>
        [Test]
        public void Create_Policy_ReturnsMatchingSelector()
        {
            Assert.IsInstanceOf<BalancedEggTypeSelector>(EggTypeSelectorFactory.Create(SpawnSelectionPolicy.Balanced));
            Assert.IsInstanceOf<RandomEggTypeSelector>(EggTypeSelectorFactory.Create(SpawnSelectionPolicy.Random));
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 지정된 종류를 갖는 Wave를 생성합니다.
        /// </summary>
        private WaveData CreateWave(params EggType[] types)
        {
            // Selector는 Types만 사용하므로 나머지 값은 결과에 영향을 주지 않음
            const int spawnCount = 1;
            const float spawnInterval = 1f;

            return new WaveData(
                spawnCount,
                spawnInterval,
                new List<EggType>(types),
                SpawnSelectionPolicy.Balanced);
        }

        /// <summary>
        /// Selector로 지정된 횟수만큼 연속 선택한 결과를 반환합니다.
        /// </summary>
        private List<EggType> SelectMany(IEggTypeSelector selector, WaveData wave, int count)
        {
            List<EggType> selected = new List<EggType>(count);

            for (int i = 0; i < count; i++)
            {
                selected.Add(selector.Select(wave));
            }

            return selected;
        }

        /// <summary>
        /// 목록에서 지정된 EggType의 개수를 반환합니다.
        /// </summary>
        private int CountOf(List<EggType> types, EggType target)
        {
            int count = 0;

            foreach (EggType type in types)
            {
                if (type == target)
                {
                    count++;
                }
            }

            return count;
        }
        #endregion
    }
}
