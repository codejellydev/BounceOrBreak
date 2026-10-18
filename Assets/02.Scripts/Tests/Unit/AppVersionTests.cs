using CodeJelly.BounceOrBreak.Core.Remote;
using NUnit.Framework;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// AppVersion의 해석과 비교 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class AppVersionTests
    {
        #region Public Methods
        /// <summary>
        /// 접미사가 있어도 숫자 부분만 해석합니다.
        /// </summary>
        [TestCase("0.1.4", "0.1.4")]
        [TestCase("0.1.4-dev.12+abc1234", "0.1.4")]
        [TestCase("0.1.4+abc1234", "0.1.4")]
        [TestCase("1.0", "1.0")]
        public void TryParse_ValidText_TakesNumericPart(string text, string expected)
        {
            bool parsed = AppVersion.TryParse(text, out AppVersion version);

            Assert.IsTrue(parsed);
            Assert.AreEqual(expected, version.ToString());
        }

        /// <summary>
        /// "major.minor" 형태가 아니면 해석에 실패합니다.
        /// </summary>
        [TestCase(null)]
        [TestCase("")]
        [TestCase("abc")]
        [TestCase("1")]
        [TestCase("-dev.1")]
        public void TryParse_InvalidText_ReturnsFalse(string text)
        {
            bool parsed = AppVersion.TryParse(text, out _);

            Assert.IsFalse(parsed);
        }

        /// <summary>
        /// 자릿수별 숫자로 비교하며 접미사는 무시합니다.
        /// </summary>
        [TestCase("0.1.3", "0.1.4", -1)]
        [TestCase("0.1.4", "0.1.4-dev.3+abc", 0)]
        [TestCase("0.2.0", "0.1.9", 1)]
        [TestCase("1.0.0", "0.9.9", 1)]
        [TestCase("0.1.10", "0.1.9", 1)]
        public void CompareTo_OrdersNumerically(string left, string right, int expectedSign)
        {
            AppVersion.TryParse(left, out AppVersion a);
            AppVersion.TryParse(right, out AppVersion b);

            Assert.AreEqual(expectedSign, System.Math.Sign(a.CompareTo(b)));
        }

        /// <summary>
        /// 해석되지 않은 기본값은 어떤 버전보다도 낮습니다.
        /// </summary>
        [Test]
        public void CompareTo_Default_IsLowest()
        {
            AppVersion.TryParse("0.0.1", out AppVersion version);

            Assert.Less(default(AppVersion).CompareTo(version), 0);
            Assert.AreEqual(0, default(AppVersion).CompareTo(default));
        }
        #endregion
    }
}