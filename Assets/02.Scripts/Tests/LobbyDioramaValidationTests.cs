using CodeJelly.BounceOrBreak.Game.Lobby;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// LobbyScene에 배치된 LobbyDiorama의 배선을 검증하는 테스트 클래스.
    /// </summary>
    public class LobbyDioramaValidationTests
    {
        #region Private Variables
        private const string LOBBY_SCENE_PATH = "Assets/01.Scenes/LobbyScene.unity";

        private Scene _scene;
        private bool _hasOpenedScene;
        #endregion


        #region Public Methods
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _scene = SceneManager.GetSceneByPath(LOBBY_SCENE_PATH);
            _hasOpenedScene = _scene.isLoaded == false;

            if (_hasOpenedScene == true)
            {
                _scene = EditorSceneManager.OpenScene(LOBBY_SCENE_PATH, OpenSceneMode.Additive);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // 이미 열려 있던 씬을 닫으면 편집 중인 변경이 사라지므로, 이 테스트가 연 경우에만 닫음
            if (_hasOpenedScene == true)
            {
                EditorSceneManager.CloseScene(_scene, true);
            }
        }

        /// <summary>
        /// LobbyScene에 유효한 LobbyDiorama가 하나 있습니다.
        /// </summary>
        [Test]
        public void LobbyScene_HasValidDiorama()
        {
            LobbyDiorama diorama = null;

            foreach (GameObject root in _scene.GetRootGameObjects())
            {
                LobbyDiorama found = root.GetComponentInChildren<LobbyDiorama>(true);

                if (found == null)
                {
                    continue;
                }

                Assert.IsNull(diorama, $"More than one LobbyDiorama found in {LOBBY_SCENE_PATH}.");
                diorama = found;
            }

            Assert.IsNotNull(diorama, $"No LobbyDiorama found in {LOBBY_SCENE_PATH}.");
            Assert.IsTrue(diorama.IsValid, $"LobbyDiorama on {diorama.name} is not valid. Assign a valid LobbyDioramaConfig.");
        }
        #endregion
    }
}