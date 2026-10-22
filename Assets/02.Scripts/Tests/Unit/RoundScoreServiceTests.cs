using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.Gameplay.Score;
using NUnit.Framework;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// RoundScoreService의 점수 누적과 초기화를 검증하는 테스트 클래스.
    /// </summary>
    public class RoundScoreServiceTests
    {
        #region Private Variables
        private RoundScoreService _service;
        #endregion


        #region Public Methods
        [SetUp]
        public void SetUp()
        {
            _service = new RoundScoreService();
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
        /// 점수를 초기화하면 현재 점수가 0이 됩니다.
        /// </summary>
        [Test]
        public void ResetScore_ClearsCurrent()
        {
            _service.AddScore();

            _service.ResetScore();

            Assert.AreEqual(0, _service.CurrentScore.CurrentValue);
        }
        #endregion
    }
}