#if UNITY_EDITOR
using CodeJelly.BounceOrBreak.Core.Logging;
using System;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CodeJelly.BounceOrBreak.Core.Editing
{
    /// <summary>
    /// DrivenProperties의 에디터 구현부.
    /// Unity 내부의 DrivenPropertyManager를 리플렉션으로 호출합니다.
    /// </summary>
    public static partial class DrivenProperties
    {
        #region Constructors
        static DrivenProperties()
        {
            if (IsValid == false)
            {
                AppLogger.Warning("[DrivenProperties] DrivenPropertyManager is not found. Values changed in Edit mode will be saved.");
            }
        }
        #endregion


        #region Properties
        /// <summary>
        /// DrivenProperties가 유효한지 확인합니다.
        /// </summary>
        public static bool IsValid => REGISTER != null && UNREGISTER_ALL != null;
        #endregion


        #region Private Variables
        private static readonly Type MANAGER_TYPE = typeof(Object).Assembly.GetType(AppConstants.DrivenProperties.MANAGER_TYPE_NAME);

        private static readonly Action<Object, Object, string> REGISTER = CreateDelegate<Action<Object, Object, string>>(AppConstants.DrivenProperties.REGISTER_METHOD_NAME, typeof(Object), typeof(Object), typeof(string));
        private static readonly Action<Object> UNREGISTER_ALL = CreateDelegate<Action<Object>>(AppConstants.DrivenProperties.UNREGISTER_ALL_METHOD_NAME, typeof(Object));
        #endregion


        #region Private Methods
        private static bool HasProperty(Object target, string propertyPath)
        {
            using (SerializedObject serializedObject = new SerializedObject(target))
            {
                return serializedObject.FindProperty(propertyPath) != null;
            }
        }

        /// <summary>
        /// DrivenPropertyManager의 메소드를 대리자로 만듭니다.
        /// </summary>
        /// <returns>메소드의 대리자. 찾지 못하면 null</returns>
        private static T CreateDelegate<T>(string methodName, params Type[] parameterTypes) where T : Delegate
        {
            MethodInfo method = MANAGER_TYPE?.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);

            return (method != null)
                ? (T)Delegate.CreateDelegate(typeof(T), method)
                : null;
        }
        #endregion

        static partial void RegisterInternal(Object driver, Object target, string propertyPath)
        {
            if (IsValid == false || driver == null || target == null)
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode == true)
            {
                return;
            }

            if (HasProperty(target, propertyPath) == false)
            {
                AppLogger.Error($"[DrivenProperties] Property '{propertyPath}' is not found on {target.GetType().Name}.");

                return;
            }

            REGISTER(driver, target, propertyPath);
        }

        static partial void UnregisterAllInternal(Object driver)
        {
            if (IsValid == false || driver == null)
            {
                return;
            }

            UNREGISTER_ALL(driver);
        }
    }
}
#endif