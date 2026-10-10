using CodeJelly.BounceOrBreak.Game.Scope;
using CodeJelly.BounceOrBreak.Game.UI.Overlay;
using NUnit.Framework;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// ApplicationScope 프리팹의 배선을 검증하는 테스트 클래스.
    /// 루트 Scope는 앱 전체의 시작점이라 배선 하나만 비어도 부팅이 멈춥니다.
    /// </summary>
    public class ApplicationScopePrefabValidationTests
    {
        #region Private Variables
        private const string APPLICATION_SCOPE_PATH = "Assets/03.Prefabs/Scope/ApplicationScope.prefab";
        #endregion


        #region Public Methods
        /// <summary>
        /// 모든 오브젝트 참조가 연결돼 있습니다.
        /// </summary>
        [Test]
        public void SerializedReferences_AreAssigned()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(APPLICATION_SCOPE_PATH);

            // 프리팹을 못 찾으면 무조건 통과하므로 로드 자체를 검증
            Assert.IsNotNull(prefab, $"No prefab found at {APPLICATION_SCOPE_PATH}.");

            ApplicationScope scope = prefab.GetComponent<ApplicationScope>();

            Assert.IsNotNull(scope, $"No ApplicationScope on {prefab.name}.");

            using (SerializedObject serializedScope = new SerializedObject(scope))
            {
                SerializedProperty property = serializedScope.GetIterator();

                while (property.NextVisible(true) == true)
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference)
                    {
                        continue;
                    }

                    Assert.IsNotNull(property.objectReferenceValue,
                        $"'{property.displayName}' is not assigned on {prefab.name}.");
                }
            }
        }

        /// <summary>
        /// ScreenFader 프리팹의 배선이 유효합니다.
        /// </summary>
        [Test]
        public void ScreenFaderPrefab_IsValid()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(APPLICATION_SCOPE_PATH);

            Assert.IsNotNull(prefab, $"No prefab found at {APPLICATION_SCOPE_PATH}.");

            ApplicationScope scope = prefab.GetComponent<ApplicationScope>();
            FieldInfo field = typeof(ApplicationScope).GetField("_screenFaderPrefab",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(field, "ApplicationScope has no _screenFaderPrefab field.");

            ScreenFader fader = field.GetValue(scope) as ScreenFader;

            Assert.IsNotNull(fader, $"ScreenFader prefab is not assigned on {prefab.name}.");
            Assert.IsTrue(fader.IsValid, "ScreenFader is not valid. Check its CanvasGroup wiring.");
        }
        #endregion
    }
}