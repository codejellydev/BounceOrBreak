using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.Gameplay.Score;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// ScoreService의 점수 산정 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class ScoreServiceTests
    {
        #region Private Variables
        private FakeSettingsRepository _repository;
        private ScoreService _service;
        #endregion


        #region Public Methods
        [SetUp]
        public void SetUp()
        {
            _repository = new FakeSettingsRepository();
            _service = new ScoreService(_repository);
        }

        [TearDown]
        public void TearDown()
        {
            _service.Dispose();
        }

        /// <summary>
        /// 득점할 때마다 현재 점수에 MATCHED만큼 누적됩니다.
        /// </summary>
        [Test]
        public void AddScore_AccumulatesMatchedScore()
        {
            const int addCount = 3;

            for (int i = 0; i < addCount; i++)
            {
                _service.AddScore();
            }

            Assert.AreEqual(GameConstants.Score.MATCHED * addCount, _service.CurrentScore.CurrentValue);
        }

        /// <summary>
        /// 점수를 초기화하면 현재 점수만 0이 되고 최고 점수는 유지됩니다.
        /// </summary>
        [Test]
        public void ResetScore_ClearsCurrentAndKeepsBest()
        {
            _service.AddScore();
            _service.TryUpdateBestScore();
            int bestBeforeReset = _service.BestScore.CurrentValue;

            _service.ResetScore();

            Assert.AreEqual(0, _service.CurrentScore.CurrentValue);
            Assert.AreEqual(bestBeforeReset, _service.BestScore.CurrentValue);
        }

        /// <summary>
        /// 현재 점수가 최고 점수보다 크면 최고 점수가 갱신됩니다.
        /// </summary>
        [Test]
        public void TryUpdateBestScore_CurrentExceedsBest_UpdatesAndReturnsTrue()
        {
            _service.AddScore();

            bool updated = _service.TryUpdateBestScore();

            Assert.IsTrue(updated);
            Assert.AreEqual(_service.CurrentScore.CurrentValue, _service.BestScore.CurrentValue);
        }

        /// <summary>
        /// 현재 점수가 최고 점수와 같으면 갱신되지 않습니다.
        /// </summary>
        [Test]
        public void TryUpdateBestScore_CurrentEqualsBest_ReturnsFalse()
        {
            _service.AddScore();
            _service.TryUpdateBestScore();
            Assert.AreEqual(_service.CurrentScore.CurrentValue, _service.BestScore.CurrentValue);

            bool updated = _service.TryUpdateBestScore();

            Assert.IsFalse(updated);
        }

        /// <summary>
        /// 현재 점수가 최고 점수보다 작으면 갱신되지 않습니다.
        /// </summary>
        [Test]
        public async Task TryUpdateBestScore_CurrentBelowBest_ReturnsFalse()
        {
            const int savedBest = 100;

            await _repository.SaveAsync(GameConstants.Score.Keys.REPOSITORY_KEY, new ScoreRecord { BestScore = savedBest });
            await _service.InitializeAsync();
            Assert.AreEqual(savedBest, _service.BestScore.CurrentValue);

            _service.AddScore();
            bool updated = _service.TryUpdateBestScore();

            Assert.IsFalse(updated);
            Assert.AreEqual(savedBest, _service.BestScore.CurrentValue);
        }

        /// <summary>
        /// 저장된 기록이 있으면 최고 점수를 불러옵니다.
        /// </summary>
        [Test]
        public async Task InitializeAsync_SavedRecordExists_LoadsBestScore()
        {
            const int savedBest = 42;

            await _repository.SaveAsync(GameConstants.Score.Keys.REPOSITORY_KEY, new ScoreRecord { BestScore = savedBest });

            await _service.InitializeAsync();

            Assert.AreEqual(savedBest, _service.BestScore.CurrentValue);
        }

        /// <summary>
        /// 저장된 기록이 없으면 최고 점수는 기본값으로 시작합니다.
        /// </summary>
        [Test]
        public async Task InitializeAsync_NoSavedRecord_KeepsDefault()
        {
            await _service.InitializeAsync();

            Assert.AreEqual(0, _service.BestScore.CurrentValue);
        }

        /// <summary>
        /// 현재 최고 점수가 저장소에 기록됩니다.
        /// </summary>
        [Test]
        public async Task SaveBestScoreAsync_SavesCurrentBest()
        {
            _service.AddScore();
            _service.TryUpdateBestScore();
            Assert.AreEqual(_service.CurrentScore.CurrentValue, _service.BestScore.CurrentValue);

            await _service.SaveBestScoreAsync();

            var loaded = await _repository.LoadAsync<ScoreRecord>(GameConstants.Score.Keys.REPOSITORY_KEY);
            Assert.IsTrue(loaded.IsSuccess);
            Assert.AreEqual(_service.BestScore.CurrentValue, loaded.Value.BestScore);
        }
        #endregion
    }
}
