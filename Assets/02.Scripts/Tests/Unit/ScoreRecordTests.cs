using CodeJelly.BounceOrBreak.Game.Gameplay.Score;
using NUnit.Framework;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// ScoreRecord의 데이터 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class ScoreRecordTests
    {
        #region Public Methods
        /// <summary>
        /// 최고 점수에 음수를 대입하면 0으로 보정됩니다.
        /// </summary>
        [Test]
        public void BestScore_NegativeValue_ClampsToZero()
        {
            ScoreRecord record = new ScoreRecord { BestScore = -1 };

            Assert.AreEqual(0, record.BestScore);
        }
        #endregion
    }
}
