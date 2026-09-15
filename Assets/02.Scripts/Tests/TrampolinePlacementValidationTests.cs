using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.Gameplay.Stage;
using CodeJelly.BounceOrBreak.Game.Trampoline;
using CodeJelly.BounceOrBreak.Game.Wall;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// Trampoline의 배치 규칙과 드로잉 경계를 검증하는 테스트 클래스.
    /// </summary>
    public class TrampolinePlacementValidationTests
    {
        #region Private Variables
        private const float TOLERANCE = 0.0001f;

        private const string GAME_SCENE_PATH = "Assets/01.Scenes/GameScene.unity";

        private Scene _scene;
        private bool _hasOpenedScene;

        private float _leftBoundary;
        private float _rightBoundary;
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

            ResolveBoundaries();
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
        /// 배치된 모든 Trampoline이 Wall을 침범하지 않습니다.
        /// </summary>
        [Test]
        public void Trampoline_StaysInsideWalls()
        {
            foreach (StageConfig config in LoadStageConfigs())
            {
                foreach (TrampolineSpawnData data in config.Trampolines)
                {
                    foreach (Vector2 point in data.Points)
                    {
                        Assert.IsTrue
                        (
                            _leftBoundary - TOLERANCE <= point.x && point.x <= _rightBoundary + TOLERANCE,
                            $"Trampoline point {point} overlaps a wall in {config.name}."
                        );
                    }
                }
            }
        }

        /// <summary>
        /// 드로잉 경계 상수가 Wall의 안쪽 면과 일치합니다.
        /// </summary>
        [Test]
        public void DrawBoundary_MatchesWallInnerFaces()
        {
            Assert.AreEqual(-GameConstants.Trampoline.DRAW_BOUNDARY_X, _leftBoundary, TOLERANCE, "Left draw boundary does not match the wall.");
            Assert.AreEqual(GameConstants.Trampoline.DRAW_BOUNDARY_X, _rightBoundary, TOLERANCE, "Right draw boundary does not match the wall.");
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 씬에 배치된 Wall로부터 플레이 영역의 좌우 경계를 구합니다.
        /// </summary>
        private void ResolveBoundaries()
        {
            List<Wall> walls = new List<Wall>();

            foreach (GameObject root in _scene.GetRootGameObjects())
            {
                walls.AddRange(root.GetComponentsInChildren<Wall>(true));
            }

            // 하나도 못 찾으면 간격이 무한대가 되어 무조건 통과하므로 탐색 자체를 검증
            Assert.IsNotEmpty(walls, $"No Wall found in {GAME_SCENE_PATH}.");

            _leftBoundary = float.NegativeInfinity;
            _rightBoundary = float.PositiveInfinity;

            foreach (Wall wall in walls)
            {
                Bounds bounds = wall.GetComponent<Collider2D>().bounds;

                // 왼쪽 벽은 max, 오른쪽 벽은 min이 안쪽 면이므로 원점을 기준으로 분기
                if (bounds.center.x < 0f)
                {
                    _leftBoundary = Mathf.Max(_leftBoundary, bounds.max.x);
                }
                else
                {
                    _rightBoundary = Mathf.Min(_rightBoundary, bounds.min.x);
                }
            }
        }

        /// <summary>
        /// 프로젝트의 모든 StageConfig를 불러오고 하나 이상 존재하는지 확인합니다.
        /// </summary>
        private static List<StageConfig> LoadStageConfigs()
        {
            List<StageConfig> configs = new List<StageConfig>(EditorAssetLoader.LoadAll<StageConfig>());

            // 에셋을 못 찾으면 무조건 통과하므로 로드 자체를 검증
            Assert.IsNotEmpty(configs, "No StageConfig asset found.");

            return configs;
        }
        #endregion
    }
}
