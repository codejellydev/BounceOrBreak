using System;

namespace CodeJelly.BounceOrBreak.Core.Logging
{
    /// <summary>
    /// 커스텀 Logger 클래스.
    /// 에디터에서는 콘솔 출력, 빌드에서는 파일 저장을 수행합니다.
    /// Unity 내부 로그도 자동으로 파일에 기록됩니다. (빌드만)
    /// TODO: ZString 도입 / Global Using Directive 활용 (언젠가 유니티가 C# 10 이상을 지원할 때)
    /// </summary>
    public static partial class AppLogger
    {
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
        }

        #region Public Methods
        /// <summary>
        /// Debug 로그를 출력합니다. (에디터 및 개발 빌드 전용)
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        [UnityEngine.HideInCallstack]
        public static void Debug(string message)
        {
            LogInternal(LogLevel.Debug, message);
        }

        /// <summary>
        /// Info 로그를 출력합니다.
        /// </summary>
        [UnityEngine.HideInCallstack]
        public static void Info(string message)
        {
            LogInternal(LogLevel.Info, message);
        }

        /// <summary>
        /// Warning 로그를 출력합니다.
        /// </summary>
        [UnityEngine.HideInCallstack]
        public static void Warning(string message)
        {
            LogInternal(LogLevel.Warning, message);
        }

        /// <summary>
        /// Error 로그를 출력합니다.
        /// </summary>
        [UnityEngine.HideInCallstack]
        public static void Error(string message)
        {
            LogInternal(LogLevel.Error, message);
        }

        /// <summary>
        /// Exception 로그를 출력합니다.
        /// </summary>
        [UnityEngine.HideInCallstack]
        public static void Exception(Exception exception)
        {
            string message = $"{exception.Message}\n{exception.StackTrace}";
            LogInternal(LogLevel.Error, message);
        }
        #endregion

        // LogInternal은 각 플랫폼별 partial class에서 구현
        static partial void LogInternal(LogLevel level, string message);
    }
}