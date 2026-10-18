using UnityEngine;

namespace CodeJelly.BounceOrBreak.Core
{
    /// <summary>
    /// 애플리케이션 수준의 공용 동작을 모은 정적 클래스.
    /// </summary>
    public static class ApplicationUtility
    {
        #region Public Methods
        /// <summary>
        /// 앱을 종료합니다. 에디터에서는 플레이 모드를 멈춥니다.
        /// </summary>
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion
    }
}