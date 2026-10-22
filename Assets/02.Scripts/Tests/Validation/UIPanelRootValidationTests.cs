using CodeJelly.BounceOrBreak.Game.UI.Message;
using CodeJelly.BounceOrBreak.Game.UI.Panel;
using NUnit.Framework;
using System.Collections.Generic;

namespace CodeJelly.BounceOrBreak.Tests.Validation
{
    /// <summary>
    /// 씬에 배치된 UIPanelRoot와 상주 패널의 배선을 검증하는 테스트 클래스.
    /// </summary>
    public class UIPanelRootValidationTests
    {
        #region Private Variables
        private static readonly string[] SCENE_PATHS =
        {
            "Assets/01.Scenes/EntryScene.unity",
            "Assets/01.Scenes/LobbyScene.unity",
            "Assets/01.Scenes/GameScene.unity",
        };

        private readonly List<TestScene> _scenes = new List<TestScene>();
        #endregion


        #region Public Methods
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            foreach (string path in SCENE_PATHS)
            {
                _scenes.Add(TestScene.Open(path));
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            foreach (TestScene scene in _scenes)
            {
                scene.Dispose();
            }

            _scenes.Clear();
        }

        /// <summary>
        /// 패널을 쓰는 씬마다 UIPanelRoot가 하나 있습니다.
        /// </summary>
        [Test]
        public void Scenes_HaveOnePanelRoot()
        {
            for (int i = 0; i < _scenes.Count; i++)
            {
                List<UIPanelRoot> roots = _scenes[i].GetComponentsInScene<UIPanelRoot>();

                Assert.AreEqual(1, roots.Count, $"Expected exactly one UIPanelRoot in {SCENE_PATHS[i]}, found {roots.Count}.");
            }
        }

        /// <summary>
        /// 상주 패널의 키는 모든 UIPanelConfig에 Resident로 선언돼 있습니다.
        /// </summary>
        [Test]
        public void ResidentPanels_AreDeclaredResidentInConfig()
        {
            List<UIPanelConfig> configs = new List<UIPanelConfig>(EditorAssetLoader.LoadAll<UIPanelConfig>());

            Assert.IsNotEmpty(configs, "No UIPanelConfig asset found.");

            for (int i = 0; i < _scenes.Count; i++)
            {
                List<UIPanelInstance> residents = _scenes[i].GetComponentsInScene<UIPanelInstance>();

                Assert.IsNotEmpty(residents, $"No resident panel in {SCENE_PATHS[i]}.");

                foreach (UIPanelInstance resident in residents)
                {
                    foreach (UIPanelConfig config in configs)
                    {
                        Assert.IsTrue(config.TryGetEntry(resident.Key, out UIPanelConfig.Entry entry), $"Resident panel '{resident.Key}' on {resident.name} is not declared in {config.name}.");
                        Assert.AreEqual(UIPanelLifetime.Resident, entry.Lifetime, $"'{resident.Key}' is placed in {SCENE_PATHS[i]} but declared {entry.Lifetime} in {config.name}.");
                    }
                }
            }
        }

        /// <summary>
        /// 상주 MessagePanelView는 UIPanelRoot 아래에 있고 유효합니다.
        /// </summary>
        [Test]
        public void ResidentMessagePanels_AreValidUnderRoot()
        {
            for (int i = 0; i < _scenes.Count; i++)
            {
                List<MessagePanelView> views = _scenes[i].GetComponentsInScene<MessagePanelView>();

                Assert.AreEqual(1, views.Count, $"Expected exactly one MessagePanelView in {SCENE_PATHS[i]}, found {views.Count}.");

                MessagePanelView view = views[0];

                Assert.IsTrue(view.IsValid, $"MessagePanelView on {view.name} in {SCENE_PATHS[i]} is not valid.");
                Assert.IsNotNull(view.GetComponentInParent<UIPanelRoot>(true), $"MessagePanelView on {view.name} in {SCENE_PATHS[i]} is not under a UIPanelRoot.");
                Assert.IsNotNull(view.GetComponent<UIPanelInstance>(), $"MessagePanelView on {view.name} in {SCENE_PATHS[i]} has no UIPanelInstance.");
            }
        }
        #endregion
    }
}