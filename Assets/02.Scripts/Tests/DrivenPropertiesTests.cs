using CodeJelly.BounceOrBreak.Core.Editing;
using NUnit.Framework;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// DrivenProperties가 Unity 내부 API에 연결되어 동작하는지 검증하는 테스트 클래스.
    /// </summary>
    public class DrivenPropertiesTests
    {
        #region Private Variables
        private const float TOLERANCE = 0.0001f;

        private const string ORTHOGRAPHIC_SIZE_PROPERTY = "orthographic size";
        private const float ORIGINAL_SIZE = 5f;
        private const float DRIVEN_SIZE = 7f;
        #endregion


        #region Public Methods
        /// <summary>
        /// DrivenProperties가 유효합니다.
        /// </summary>
        [Test]
        public void DrivenProperties_IsValid()
        {
            Assert.IsTrue(DrivenProperties.IsValid, "DrivenPropertyManager is not found. Unity may have changed its internal API.");
        }

        /// <summary>
        /// 등록한 속성은 해제하면 등록 시점의 값으로 돌아갑니다.
        /// </summary>
        [Test]
        public void UnregisterAll_RestoresRegisteredValue()
        {
            GameObject gameObject = new GameObject(nameof(DrivenPropertiesTests));

            try
            {
                Camera camera = gameObject.AddComponent<Camera>();
                camera.orthographicSize = ORIGINAL_SIZE;

                DrivenProperties.Register(gameObject, camera, ORTHOGRAPHIC_SIZE_PROPERTY);
                camera.orthographicSize = DRIVEN_SIZE;
                DrivenProperties.UnregisterAll(gameObject);

                Assert.AreEqual(ORIGINAL_SIZE, camera.orthographicSize, TOLERANCE, "The driven value was not reverted.");
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
        #endregion
    }
}