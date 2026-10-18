using System;

namespace CodeJelly.BounceOrBreak.Core.Remote
{
    /// <summary>
    /// 비교 가능한 앱 버전 값.
    /// 사전 출시·빌드 메타데이터 접미사를 뗀 숫자(ex. "0.1.4-dev.12+abc1234"의 0.1.4)로 비교합니다.
    /// </summary>
    public readonly struct AppVersion : IComparable<AppVersion>
    {
        #region Constructors
        private AppVersion(Version version)
        {
            _version = version;
        }
        #endregion


        #region Private Variables
        /// <summary>
        /// 접미사 구분자. 사전 출시 표기(-)와 빌드 메타데이터(+)
        /// </summary>
        private static readonly char[] SUFFIX_SEPARATORS = { '-', '+' };

        private readonly Version _version;
        #endregion


        #region Public Methods
        /// <summary>
        /// 버전 문자열을 해석합니다. 접미사는 무시하며 "major.minor" 이상이어야 합니다.
        /// </summary>
        public static bool TryParse(string text, out AppVersion version)
        {
            version = default;

            if (string.IsNullOrEmpty(text) == true)
            {
                return false;
            }

            int suffixIndex = text.IndexOfAny(SUFFIX_SEPARATORS);
            string core = suffixIndex < 0 ? text : text.Substring(0, suffixIndex);

            if (Version.TryParse(core, out Version parsed) == false)
            {
                return false;
            }

            version = new AppVersion(parsed);

            return true;
        }

        /// <summary>
        /// IComparable.CompareTo Implementation
        /// </summary>
        public int CompareTo(AppVersion other)
        {
            // 해석되지 않은 기본값은 가장 낮은 버전으로 취급
            if (_version == null)
            {
                return other._version == null ? 0 : -1;
            }

            if (other._version == null)
            {
                return 1;
            }

            return _version.CompareTo(other._version);
        }

        public override string ToString()
        {
            return _version == null ? string.Empty : _version.ToString();
        }
        #endregion
    }
}