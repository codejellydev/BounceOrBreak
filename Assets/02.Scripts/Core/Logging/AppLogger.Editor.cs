#if UNITY_EDITOR
using System;

namespace CodeJelly.BounceOrBreak.Core.Logging
{
    /// <summary>
    /// AppLogger의 에디터 구현부.
    /// </summary>
    public static partial class AppLogger
    {
        /// <summary>
        /// 실제 로그 처리를 수행하는 내부 메서드.
        /// 에디터에서는 Unity 콘솔에 출력합니다.
        /// </summary>
        [UnityEngine.HideInCallstack]
        static partial void LogInternal(LogLevel level, string message)
        {
            try
            {
                switch (level)
                {
                    case LogLevel.Debug:
                    case LogLevel.Info:
                        UnityEngine.Debug.Log(message);
                        break;
                    case LogLevel.Warning:
                        UnityEngine.Debug.LogWarning(message);
                        break;
                    case LogLevel.Error:
                        UnityEngine.Debug.LogError(message);
                        break;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }
    }
}
#endif