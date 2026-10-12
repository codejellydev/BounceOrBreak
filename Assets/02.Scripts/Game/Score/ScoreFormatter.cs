using System.Globalization;

namespace CodeJelly.BounceOrBreak.Game.Score
{
    /// <summary>
    /// 점수를 화면 표시용 문자열로 바꾸는 클래스.
    /// 0 이상의 점수를 전제로 합니다.
    /// </summary>
    public static class ScoreFormatter
    {
        #region Public Methods
        /// <summary>
        /// 점수를 표시용 문자열로 바꿉니다.
        /// </summary>
        public static string Format(int score)
        {
            if (score < GameConstants.Score.Abbreviation.THRESHOLD)
            {
                return score.ToString(CultureInfo.InvariantCulture);
            }

            foreach ((int divisor, string suffix) in GameConstants.Score.Abbreviation.UNITS)
            {
                if (divisor <= score)
                {
                    return Abbreviate(score, divisor, suffix);
                }
            }

            return score.ToString(CultureInfo.InvariantCulture);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 점수를 단위로 나눠 소수 한 자리까지 표시합니다. 소수부가 0이면 생략합니다.
        /// </summary>
        private static string Abbreviate(int score, int divisor, string suffix)
        {
            // 실제보다 높게 보이지 않도록 소수 첫째 자리 아래는 버림
            int tenths = score / (divisor / 10);
            int whole = tenths / 10;
            int fraction = tenths % 10;

            string wholeText = whole.ToString(CultureInfo.InvariantCulture);

            if (fraction == 0)
            {
                return wholeText + suffix;
            }

            return wholeText + "." + fraction.ToString(CultureInfo.InvariantCulture) + suffix;
        }
        #endregion
    }
}