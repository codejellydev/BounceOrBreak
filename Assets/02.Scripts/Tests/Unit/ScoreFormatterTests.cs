using CodeJelly.BounceOrBreak.Game.Score;
using NUnit.Framework;
using System.Globalization;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// ScoreFormatter의 점수 표시 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class ScoreFormatterTests
    {
        #region Public Methods
        /// <summary>
        /// 축약 기준 미만의 점수는 숫자 그대로 표시합니다.
        /// </summary>
        [TestCase(0, "0")]
        [TestCase(9999, "9999")]
        public void Format_BelowThreshold_ReturnsPlainNumber(int score, string expected)
        {
            Assert.AreEqual(expected, ScoreFormatter.Format(score));
        }

        /// <summary>
        /// 축약 기준 이상의 점수는 단위를 붙이고 소수 한 자리까지 내림해 표시합니다.
        /// </summary>
        [TestCase(10999, "10.9K")]
        [TestCase(999999, "999.9K")]
        [TestCase(1250000, "1.2M")]
        [TestCase(int.MaxValue, "2.1B")]
        public void Format_AboveThreshold_AbbreviatesWithOneDecimal(int score, string expected)
        {
            Assert.AreEqual(expected, ScoreFormatter.Format(score));
        }

        /// <summary>
        /// 소수부가 0이면 떼고 표시합니다.
        /// </summary>
        [TestCase(10000, "10K")]
        [TestCase(1000000, "1M")]
        public void Format_ZeroFraction_DropsFraction(int score, string expected)
        {
            Assert.AreEqual(expected, ScoreFormatter.Format(score));
        }

        /// <summary>
        /// 기기 언어와 관계없이 소수점은 마침표로 표시합니다.
        /// </summary>
        [Test]
        public void Format_CommaDecimalCulture_UsesPeriod()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                Assert.AreEqual("10.9K", ScoreFormatter.Format(10999));
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }
        #endregion
    }
}