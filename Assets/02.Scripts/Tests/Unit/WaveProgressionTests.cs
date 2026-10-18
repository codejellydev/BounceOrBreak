using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using NUnit.Framework;
using System.Collections.Generic;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// WaveProgression의 진행 계산 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class WaveProgressionTests
    {
        #region Private Variables
        private const int WAVE_COUNT = 2;

        private const int FIRST_WAVE_INDEX = 0;
        private const int FIRST_WAVE_SPAWN_COUNT = 3;
        private const float FIRST_WAVE_INTERVAL = 4f;

        private const int SECOND_WAVE_INDEX = 1;
        private const int SECOND_WAVE_SPAWN_COUNT = 2;
        private const float SECOND_WAVE_INTERVAL = 2f;

        private const float DECAY_PER_SPAWN = 0.5f;
        private const float MIN_INTERVAL = 1f;

        private const float TOLERANCE = 0.0001f;
        #endregion


        #region Public Methods
        /// <summary>
        /// Wave 목록이 null이면 빈 진행으로 정규화됩니다.
        /// </summary>
        [Test]
        public void Constructor_NullWaves_NormalizesToEmpty()
        {
            const int expectedCount = 0;

            WaveProgression progression = new WaveProgression(null, DECAY_PER_SPAWN, MIN_INTERVAL);

            Assert.IsNotNull(progression.Waves);
            Assert.AreEqual(expectedCount, progression.WaveCount);
            Assert.AreEqual(expectedCount, progression.TotalSpawnCount);
        }

        /// <summary>
        /// 정의된 Wave 수를 그대로 반환합니다.
        /// </summary>
        [Test]
        public void WaveCount_ReturnsDefinedWaveCount()
        {
            WaveProgression progression = CreateProgression();

            Assert.AreEqual(WAVE_COUNT, progression.WaveCount);
        }

        /// <summary>
        /// 모든 Wave의 스폰 횟수 합이 TotalSpawnCount와 일치합니다.
        /// </summary>
        [Test]
        public void TotalSpawnCount_SumsEveryWave()
        {
            const int expectedTotal = FIRST_WAVE_SPAWN_COUNT + SECOND_WAVE_SPAWN_COUNT;

            WaveProgression progression = CreateProgression();

            Assert.AreEqual(expectedTotal, progression.TotalSpawnCount);
        }

        /// <summary>
        /// 누적 스폰 횟수가 Wave의 담당 구간 안이면 해당 Wave를 가리킵니다.
        /// </summary>
        [Test]
        public void GetWaveIndex_WithinWave_ReturnsThatWave()
        {
            const int firstSpawn = 0;
            const int firstWaveLastSpawn = FIRST_WAVE_SPAWN_COUNT - 1;
            const int secondWaveFirstSpawn = FIRST_WAVE_SPAWN_COUNT;
            const int secondWaveLastSpawn = FIRST_WAVE_SPAWN_COUNT + SECOND_WAVE_SPAWN_COUNT - 1;

            WaveProgression progression = CreateProgression();

            Assert.AreEqual(FIRST_WAVE_INDEX, progression.GetWaveIndex(firstSpawn));
            Assert.AreEqual(FIRST_WAVE_INDEX, progression.GetWaveIndex(firstWaveLastSpawn));
            Assert.AreEqual(SECOND_WAVE_INDEX, progression.GetWaveIndex(secondWaveFirstSpawn));
            Assert.AreEqual(SECOND_WAVE_INDEX, progression.GetWaveIndex(secondWaveLastSpawn));
        }

        /// <summary>
        /// 마지막 Wave를 넘어서면 마지막 Wave가 계속 이어집니다.
        /// </summary>
        [Test]
        public void GetWaveIndex_BeyondLastWave_ReturnsLastWave()
        {
            const int farBeyondCount = 100;

            WaveProgression progression = CreateProgression();

            Assert.AreEqual(SECOND_WAVE_INDEX, progression.GetWaveIndex(progression.TotalSpawnCount));
            Assert.AreEqual(SECOND_WAVE_INDEX, progression.GetWaveIndex(progression.TotalSpawnCount + farBeyondCount));
        }

        /// <summary>
        /// 누적 스폰 횟수에 해당하는 Wave 인스턴스를 반환합니다.
        /// </summary>
        [Test]
        public void GetWave_ReturnsWaveAtResolvedIndex()
        {
            const int firstSpawn = 0;
            const int secondWaveFirstSpawn = FIRST_WAVE_SPAWN_COUNT;

            WaveProgression progression = CreateProgression();

            Assert.AreSame(progression.Waves[FIRST_WAVE_INDEX], progression.GetWave(firstSpawn));
            Assert.AreSame(progression.Waves[SECOND_WAVE_INDEX], progression.GetWave(secondWaveFirstSpawn));
        }

        /// <summary>
        /// Wave 구간 안에서는 해당 Wave의 스폰 간격을 그대로 사용합니다.
        /// </summary>
        [Test]
        public void GetSpawnInterval_WithinWave_ReturnsWaveInterval()
        {
            const int firstSpawn = 0;
            const int secondWaveFirstSpawn = FIRST_WAVE_SPAWN_COUNT;

            WaveProgression progression = CreateProgression();

            Assert.AreEqual(FIRST_WAVE_INTERVAL, progression.GetSpawnInterval(firstSpawn), TOLERANCE);
            Assert.AreEqual(SECOND_WAVE_INTERVAL, progression.GetSpawnInterval(secondWaveFirstSpawn), TOLERANCE);
        }

        /// <summary>
        /// 마지막 Wave를 넘어선 뒤에는 Egg 하나마다 스폰 간격이 줄어듭니다.
        /// </summary>
        [Test]
        public void GetSpawnInterval_BeyondLastWave_DecaysPerSpawn()
        {
            const float expectedAfterOneSpawn = 1.5f;

            WaveProgression progression = CreateProgression();
            int total = progression.TotalSpawnCount;

            // 초과분이 0이면 아직 감쇠하지 않음
            Assert.AreEqual(SECOND_WAVE_INTERVAL, progression.GetSpawnInterval(total), TOLERANCE);
            Assert.AreEqual(expectedAfterOneSpawn, progression.GetSpawnInterval(total + 1), TOLERANCE);
        }

        /// <summary>
        /// 감쇠가 누적되어도 하한 아래로는 내려가지 않습니다.
        /// </summary>
        [Test]
        public void GetSpawnInterval_LongBeyondLastWave_ClampsToMinimum()
        {
            const int farBeyondCount = 1000;

            WaveProgression progression = CreateProgression();

            Assert.AreEqual(MIN_INTERVAL, progression.GetSpawnInterval(progression.TotalSpawnCount + farBeyondCount), TOLERANCE);
        }

        /// <summary>
        /// Wave에 정의된 간격이 하한보다 낮으면 하한으로 보정됩니다.
        /// </summary>
        [Test]
        public void GetSpawnInterval_WaveIntervalBelowMinimum_ClampsToMinimum()
        {
            const float belowMinimumInterval = 0.2f;
            const int firstSpawn = 0;

            List<WaveData> waves = new List<WaveData>
            {
                CreateWave(FIRST_WAVE_SPAWN_COUNT, belowMinimumInterval)
            };

            WaveProgression progression = new WaveProgression(waves, DECAY_PER_SPAWN, MIN_INTERVAL);

            Assert.AreEqual(MIN_INTERVAL, progression.GetSpawnInterval(firstSpawn), TOLERANCE);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 테스트가 공유하는 진행을 생성합니다.
        /// </summary>
        private WaveProgression CreateProgression()
        {
            List<WaveData> waves = new List<WaveData>
            {
                CreateWave(FIRST_WAVE_SPAWN_COUNT, FIRST_WAVE_INTERVAL),
                CreateWave(SECOND_WAVE_SPAWN_COUNT, SECOND_WAVE_INTERVAL)
            };

            return new WaveProgression(waves, DECAY_PER_SPAWN, MIN_INTERVAL);
        }

        /// <summary>
        /// 지정된 스폰 횟수와 간격을 갖는 Wave를 생성합니다.
        /// </summary>
        private WaveData CreateWave(int spawnCount, float spawnInterval)
        {
            return new WaveData(
                spawnCount,
                spawnInterval,
                new List<EggType> { EggType.Basic },
                SpawnSelectionPolicy.Balanced);
        }
        #endregion
    }
}
