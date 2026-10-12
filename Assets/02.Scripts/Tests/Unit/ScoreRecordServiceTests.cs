using CodeJelly.BounceOrBreak.Core.Settings;
using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.Score;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// ScoreRecordService의 기록 관리 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class ScoreRecordServiceTests
    {
        #region Private Variables
        private const string OTHER_KEY = "Other";

        private FakeSettingsRepository _repository;
        private ScoreRecordService _service;
        #endregion


        #region Public Methods
        [SetUp]
        public void SetUp()
        {
            _repository = new FakeSettingsRepository();
            _service = new ScoreRecordService(_repository);
        }

        [TearDown]
        public void TearDown()
        {
            _service.Dispose();
        }

        /// <summary>
        /// 저장된 기록이 있으면 최고 점수를 불러옵니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_SavedRecordExists_LoadsBestScore()
        {
            const int savedBest = 42;

            await _repository.SaveAsync(GameConstants.Score.Keys.REPOSITORY_KEY, new ScoreRecord { BestScore = savedBest });

            await _service.LoadAsync();

            Assert.AreEqual(savedBest, _service.BestScore.CurrentValue);
        }

        /// <summary>
        /// 저장된 기록이 없으면 최고 점수는 기본값으로 시작합니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_NoSavedRecord_KeepsDefault()
        {
            await _service.LoadAsync();

            Assert.AreEqual(0, _service.BestScore.CurrentValue);
        }

        /// <summary>
        /// 점수가 최고 점수보다 크면 최고 점수가 갱신됩니다.
        /// </summary>
        [Test]
        public void TryUpdateBestScore_ScoreExceedsBest_UpdatesAndReturnsTrue()
        {
            const int score = 10;

            bool updated = _service.TryUpdateBestScore(score);

            Assert.IsTrue(updated);
            Assert.AreEqual(score, _service.BestScore.CurrentValue);
        }

        /// <summary>
        /// 점수가 최고 점수와 같으면 갱신되지 않습니다.
        /// </summary>
        [Test]
        public void TryUpdateBestScore_ScoreEqualsBest_ReturnsFalse()
        {
            const int score = 10;

            _service.TryUpdateBestScore(score);

            bool updated = _service.TryUpdateBestScore(score);

            Assert.IsFalse(updated);
        }

        /// <summary>
        /// 점수가 최고 점수보다 작으면 갱신되지 않습니다.
        /// </summary>
        [Test]
        public void TryUpdateBestScore_ScoreBelowBest_ReturnsFalse()
        {
            const int best = 100;

            _service.TryUpdateBestScore(best);

            bool updated = _service.TryUpdateBestScore(best - 1);

            Assert.IsFalse(updated);
            Assert.AreEqual(best, _service.BestScore.CurrentValue);
        }

        /// <summary>
        /// 현재 최고 점수가 저장소에 기록됩니다.
        /// </summary>
        [Test]
        public async Task SaveAsync_SavesCurrentBest()
        {
            const int best = 10;

            _service.TryUpdateBestScore(best);

            await _service.SaveAsync();

            var loaded = await _repository.LoadAsync<ScoreRecord>(GameConstants.Score.Keys.REPOSITORY_KEY);
            Assert.IsTrue(loaded.IsSuccess);
            Assert.AreEqual(best, loaded.Value.BestScore);
        }

        /// <summary>
        /// 기록을 지우면 저장소에서 ScoreRecord가 사라지고 최고 점수는 기본값으로 돌아갑니다.
        /// </summary>
        [Test]
        public async Task ClearAsync_RemovesStoredRecordAndResetsBest()
        {
            const int best = 10;

            await _repository.SaveAsync(GameConstants.Score.Keys.REPOSITORY_KEY, new ScoreRecord { BestScore = best });
            await _service.LoadAsync();

            await _service.ClearAsync();

            SettingsResult<ScoreRecord> result = await _repository.LoadAsync<ScoreRecord>(GameConstants.Score.Keys.REPOSITORY_KEY);
            Assert.AreEqual(SettingsResultError.NotFound, result.Error);
            Assert.AreEqual(0, _service.BestScore.CurrentValue);
        }

        /// <summary>
        /// 기록을 지워도 다른 키의 데이터는 남습니다.
        /// </summary>
        [Test]
        public async Task ClearAsync_KeepsOtherKeys()
        {
            await _repository.SaveAsync(OTHER_KEY, new ScoreRecord { BestScore = 10 });

            await _service.ClearAsync();

            SettingsResult<ScoreRecord> result = await _repository.LoadAsync<ScoreRecord>(OTHER_KEY);
            Assert.IsTrue(result.IsSuccess);
        }
        #endregion
    }
}