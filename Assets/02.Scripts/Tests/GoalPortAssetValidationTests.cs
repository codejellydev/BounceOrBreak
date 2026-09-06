using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using CodeJelly.BounceOrBreak.Game.Gameplay.Goal;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// GoalPort 관련 에셋 데이터의 유효성을 검증하는 테스트 클래스.
    /// </summary>
    public class GoalPortAssetValidationTests
    {
        #region Private Variables
        private const string GAME_SCENE_PATH = "Assets/01.Scenes/GameScene.unity";

        private Scene _scene;
        private bool _hasOpenedScene;
        #endregion


        #region Public Methods
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _scene = SceneManager.GetSceneByPath(GAME_SCENE_PATH);
            _hasOpenedScene = _scene.isLoaded == false;

            if (_hasOpenedScene == true)
            {
                _scene = EditorSceneManager.OpenScene(GAME_SCENE_PATH, OpenSceneMode.Additive);
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
        /// 모든 GoalPortConfig에 모든 EggColor의 스프라이트가 지정되어 있습니다.
        /// </summary>
        [Test]
        public void GoalPortConfig_HasSpriteForEveryColor()
        {
            List<GoalPortConfig> configs = new List<GoalPortConfig>(EditorAssetLoader.LoadAll<GoalPortConfig>());

            Assert.IsNotEmpty(configs, "No GoalPortConfig asset found.");

            foreach (GoalPortConfig config in configs)
            {
                foreach (EggColor color in Enum.GetValues(typeof(EggColor)))
                {
                    Assert.IsTrue(config.GetSprite(color) != null, $"Sprite for {color} is not assigned on {config.name}.");
                }
            }
        }

        /// <summary>
        /// GameScene에 배치된 모든 GoalPort가 유효합니다.
        /// </summary>
        [Test]
        public void GoalPort_IsValid()
        {
            List<GoalPort> ports = FindGoalPorts();

            // 하나도 못 찾으면 무조건 통과하므로 탐색 자체를 검증
            Assert.IsNotEmpty(ports, $"No GoalPort found in {GAME_SCENE_PATH}.");

            foreach (GoalPort port in ports)
            {
                Assert.IsTrue(port.IsValid, $"{port.name} is not valid. Check its renderer and config wiring.");
            }
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 열어둔 씬에 배치된 모든 GoalPort를 찾습니다.
        /// </summary>
        private List<GoalPort> FindGoalPorts()
        {
            List<GoalPort> ports = new List<GoalPort>();

            foreach (GameObject root in _scene.GetRootGameObjects())
            {
                ports.AddRange(root.GetComponentsInChildren<GoalPort>(true));
            }

            return ports;
        }
        #endregion
    }
}
