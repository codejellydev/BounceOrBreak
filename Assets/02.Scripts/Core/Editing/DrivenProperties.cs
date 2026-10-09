using UnityEngine;

namespace CodeJelly.BounceOrBreak.Core.Editing
{
    /// <summary>
    /// 에디터에서 임시로 바꾸는 속성을 저장에서 제외하는 클래스.
    /// 등록한 속성은 바뀐 값이 씬과 프리팹에 저장되지 않고, 해제하면 등록 시점의 값으로 돌아갑니다.
    /// </summary>
    public static partial class DrivenProperties
    {
        #region Public Methods
        /// <summary>
        /// target의 속성을 저장에서 제외합니다.
        /// </summary>
        /// <param name="driver">속성을 바꾸는 주체</param>
        /// <param name="target">속성을 가진 오브젝트</param>
        /// <param name="propertyPath">속성의 SerializedProperty 경로</param>
        public static void Register(Object driver, Object target, string propertyPath)
        {
            RegisterInternal(driver, target, propertyPath);
        }

        /// <summary>
        /// driver가 등록한 속성을 모두 해제합니다.
        /// </summary>
        public static void UnregisterAll(Object driver)
        {
            UnregisterAllInternal(driver);
        }
        #endregion

        static partial void RegisterInternal(Object driver, Object target, string propertyPath);

        static partial void UnregisterAllInternal(Object driver);
    }
}