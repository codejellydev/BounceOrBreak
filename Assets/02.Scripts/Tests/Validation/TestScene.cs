using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeJelly.BounceOrBreak.Tests.Validation
{
    /// <summary>
    /// 테스트가 검사할 씬을 Additive로 열어 두는 클래스.
    /// </summary>
    internal sealed class TestScene : IDisposable
    {
        #region Constructors
        private TestScene(Scene scene, bool hasOpened)
        {
            _scene = scene;
            _hasOpened = hasOpened;
        }
        #endregion


        #region Private Variables
        private readonly Scene _scene;
        private readonly bool _hasOpened;
        #endregion


        #region Public Methods
        /// <summary>
        /// 경로의 씬을 엽니다. 이미 열려 있으면 그대로 씁니다.
        /// </summary>
        public static TestScene Open(string path)
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            bool hasOpened = scene.isLoaded == false;

            if (hasOpened == true)
            {
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }

            return new TestScene(scene, hasOpened);
        }

        /// <summary>
        /// 열어둔 씬의 루트 GameObject를 반환합니다.
        /// </summary>
        public GameObject[] GetRootGameObjects()
        {
            return _scene.GetRootGameObjects();
        }

        /// <summary>
        /// 열어둔 씬에 배치된 모든 T 컴포넌트를 찾습니다.
        /// </summary>
        public List<T> GetComponentsInScene<T>() where T : Component
        {
            List<T> components = new List<T>();

            foreach (GameObject root in _scene.GetRootGameObjects())
            {
                components.AddRange(root.GetComponentsInChildren<T>(true));
            }

            return components;
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            // 이미 열려 있던 씬을 닫으면 편집 중인 변경이 사라지므로, 이 객체가 연 경우에만 닫음
            if (_hasOpened == true)
            {
                EditorSceneManager.CloseScene(_scene, true);
            }
        }
        #endregion
    }
}