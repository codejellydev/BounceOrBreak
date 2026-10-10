using CodeJelly.BounceOrBreak.Core.Viewport;
using CodeJelly.BounceOrBreak.Game.Gameplay.Goal;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests.Validation
{
    /// <summary>
    /// GameScene에 배치된 GoalPort의 배치 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class GoalPortPlacementValidationTests
    {
        #region Private Variables
        private const float TOLERANCE = 0.0001f;

        private const string GAME_SCENE_PATH = "Assets/01.Scenes/GameScene.unity";

        private TestScene _scene;
        #endregion


        #region Public Methods
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _scene = TestScene.Open(GAME_SCENE_PATH);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _scene?.Dispose();
        }

        /// <summary>
        /// 모든 GoalPort가 어떤 화면비에서도 보이도록 보장 영역 안에 있습니다.
        /// </summary>
        [Test]
        public void GoalPort_StaysInsideGuaranteedArea()
        {
            List<ViewportConfig> configs = new List<ViewportConfig>(EditorAssetLoader.LoadAll<ViewportConfig>());

            // 에셋을 못 찾으면 무조건 통과하므로 로드 자체를 검증. 여럿이면 어느 것을 기준으로 삼을지 정할 수 없어 하나로 제한
            Assert.AreEqual(1, configs.Count, "Expected exactly one ViewportConfig asset.");

            List<GoalPort> ports = _scene.GetComponentsInScene<GoalPort>();

            // 하나도 못 찾으면 무조건 통과하므로 탐색 자체를 검증
            Assert.IsNotEmpty(ports, $"No GoalPort found in {GAME_SCENE_PATH}.");

            ViewportContract contract = configs[0].Contract;

            foreach (GoalPort port in ports)
            {
                // 보여야 하는지가 기준이므로 collider가 아닌 renderer의 bounds를 사용
                Bounds bounds = port.GetComponentInChildren<SpriteRenderer>(true).bounds;

                Assert.IsTrue
                (
                    -contract.HalfWidth - TOLERANCE <= bounds.min.x && bounds.max.x <= contract.HalfWidth + TOLERANCE
                    && -contract.HalfHeight - TOLERANCE <= bounds.min.y && bounds.max.y <= contract.HalfHeight + TOLERANCE,
                    $"{port.name} leaves the guaranteed area. bounds: {bounds.min} ~ {bounds.max}, area: x ±{contract.HalfWidth}, y ±{contract.HalfHeight}."
                );
            }
        }
        #endregion
    }
}
