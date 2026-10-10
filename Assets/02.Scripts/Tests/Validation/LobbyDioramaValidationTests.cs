using CodeJelly.BounceOrBreak.Game.Lobby;
using NUnit.Framework;
using System.Collections.Generic;

namespace CodeJelly.BounceOrBreak.Tests.Validation
{
    /// <summary>
    /// LobbyScene에 배치된 LobbyDiorama의 배선을 검증하는 테스트 클래스.
    /// </summary>
    public class LobbyDioramaValidationTests
    {
        #region Private Variables
        private const string LOBBY_SCENE_PATH = "Assets/01.Scenes/LobbyScene.unity";

        private TestScene _scene;
        #endregion


        #region Public Methods
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _scene = TestScene.Open(LOBBY_SCENE_PATH);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _scene?.Dispose();
        }

        /// <summary>
        /// LobbyScene에 유효한 LobbyDiorama가 하나 있습니다.
        /// </summary>
        [Test]
        public void LobbyScene_HasValidDiorama()
        {
            List<LobbyDiorama> dioramas = _scene.GetComponentsInScene<LobbyDiorama>();

            Assert.AreEqual(1, dioramas.Count, $"Expected exactly one LobbyDiorama in {LOBBY_SCENE_PATH}, found {dioramas.Count}.");

            LobbyDiorama diorama = dioramas[0];

            Assert.IsTrue(diorama.IsValid, $"LobbyDiorama on {diorama.name} is not valid. Assign a valid LobbyDioramaConfig.");
        }
        #endregion
    }
}