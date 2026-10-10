using CodeJelly.BounceOrBreak.Core.Viewport;
using CodeJelly.BounceOrBreak.Game.Wall;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeJelly.BounceOrBreak.Tests.Validation
{
    /// <summary>
    /// GameScene에 배치된 Wall의 좌우 배치 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class WallPlacementValidationTests
    {
        #region Private Variables
        private const float TOLERANCE = 0.0001f;

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
        /// 모든 Wall의 바깥면이 보장 영역의 좌우 경계와 일치합니다.
        /// </summary>
        [Test]
        public void Wall_OuterFaceMatchesGuaranteedBoundary()
        {
            List<ViewportConfig> configs = new List<ViewportConfig>(EditorAssetLoader.LoadAll<ViewportConfig>());

            // 에셋을 못 찾으면 무조건 통과하므로 로드 자체를 검증. 여럿이면 어느 것을 기준으로 삼을지 정할 수 없어 하나로 제한
            Assert.AreEqual(1, configs.Count, "Expected exactly one ViewportConfig asset.");

            List<Wall> walls = FindWalls();

            // 하나도 못 찾으면 무조건 통과하므로 탐색 자체를 검증
            Assert.IsNotEmpty(walls, $"No Wall found in {GAME_SCENE_PATH}.");

            float halfWidth = configs[0].Contract.HalfWidth;

            foreach (Wall wall in walls)
            {
                Bounds bounds = wall.GetComponent<Collider2D>().bounds;

                // 좌우 벽은 바깥면의 방향이 서로 반대이므로 원점에서 먼 쪽 면을 취함
                float outerFace = Mathf.Max(Mathf.Abs(bounds.min.x), Mathf.Abs(bounds.max.x));

                Assert.AreEqual(halfWidth, outerFace, TOLERANCE, $"{wall.name} does not meet the guaranteed boundary.");
            }
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 열어둔 씬에 배치된 모든 Wall을 찾습니다.
        /// </summary>
        private List<Wall> FindWalls()
        {
            List<Wall> walls = new List<Wall>();

            foreach (GameObject root in _scene.GetRootGameObjects())
            {
                walls.AddRange(root.GetComponentsInChildren<Wall>(true));
            }

            return walls;
        }
        #endregion
    }
}
