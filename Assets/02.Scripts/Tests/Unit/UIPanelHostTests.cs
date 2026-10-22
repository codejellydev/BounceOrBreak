using CodeJelly.BounceOrBreak.Game.UI.Panel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// UIPanelHost의 상주 인스턴스 처리를 검증하는 테스트 클래스.
    /// </summary>
    public class UIPanelHostTests
    {
        #region Nested Classes
        /// <summary>
        /// 키만 반환하는 테스트용 <see cref="IUIPanel"/> 구현체.
        /// </summary>
        private sealed class StubPanel : IUIPanel
        {
            public StubPanel(string key)
            {
                Key = key;
            }

            public string Key { get; }

            public void Cancel()
            { }
        }
        #endregion


        #region Private Variables
        private const string KEY = "Test";
        private const string MISSING_KEY = "Missing";

        private GameObject _root;
        private GameObject _resident;
        private IObjectResolver _resolver;
        private UIPanelConfig _config;
        private UIPanelConfig.Entry _entry;
        private UIPanelConfig.Entry _missingEntry;
        #endregion


        #region Public Methods
        [SetUp]
        public void SetUp()
        {
            _root = new GameObject(nameof(UIPanelHostTests));
            _resident = new GameObject("Resident");
            _resident.transform.SetParent(_root.transform);

            SerializedObject instance = new SerializedObject(_resident.AddComponent<UIPanelInstance>());
            instance.FindProperty("_key").stringValue = KEY;
            instance.ApplyModifiedPropertiesWithoutUndo();

            _config = ScriptableObject.CreateInstance<UIPanelConfig>();
            SerializedObject config = new SerializedObject(_config);
            SerializedProperty entries = config.FindProperty("_entries");
            entries.InsertArrayElementAtIndex(0);
            entries.GetArrayElementAtIndex(0).FindPropertyRelative("_key").stringValue = KEY;
            entries.GetArrayElementAtIndex(0).FindPropertyRelative("_lifetime").enumValueIndex = (int)UIPanelLifetime.Transient;
            entries.InsertArrayElementAtIndex(1);
            entries.GetArrayElementAtIndex(1).FindPropertyRelative("_key").stringValue = MISSING_KEY;
            entries.GetArrayElementAtIndex(1).FindPropertyRelative("_lifetime").enumValueIndex = (int)UIPanelLifetime.Resident;
            config.ApplyModifiedPropertiesWithoutUndo();
            _config.TryGetEntry(KEY, out _entry);
            _config.TryGetEntry(MISSING_KEY, out _missingEntry);

            _resolver = new ContainerBuilder().Build();
        }

        [TearDown]
        public void TearDown()
        {
            _resolver.Dispose();
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_root);
        }

        /// <summary>
        /// 상주 인스턴스가 있으면 로드 없이 비활성 상태로 반환합니다.
        /// </summary>
        [Test]
        public async Task AcquireAsync_ResidentInstance_ReturnsItWithoutLoading()
        {
            UIPanelHost host = CreateHost(new StubPanel(KEY));

            GameObject acquired = await host.AcquireAsync(_entry, CancellationToken.None);

            Assert.AreSame(_resident, acquired);
            Assert.IsFalse(acquired.activeSelf);
        }

        /// <summary>
        /// 상주 인스턴스는 Transient로 선언돼 있어도 파괴하지 않고 비활성화만 합니다.
        /// </summary>
        [Test]
        public void Release_ResidentInstance_DeactivatesWithoutDestroying()
        {
            UIPanelHost host = CreateHost(new StubPanel(KEY));
            _resident.SetActive(true);

            host.Release(_entry, _resident);

            Assert.IsTrue(_resident != null);
            Assert.IsFalse(_resident.activeSelf);
        }

        /// <summary>
        /// Resident 선언에 상주 인스턴스가 없으면 예외를 던집니다.
        /// </summary>
        [Test]
        public void AcquireAsync_ResidentDeclaredButMissing_Throws()
        {
            UIPanelHost host = CreateHost(new StubPanel(KEY), new StubPanel(MISSING_KEY));

            Assert.Throws<System.InvalidOperationException>(() => host.AcquireAsync(_missingEntry, CancellationToken.None));
        }

        /// <summary>
        /// 등록된 Presenter가 없는 상주 인스턴스는 오류를 남기고 무시합니다.
        /// </summary>
        [Test]
        public void Constructor_ResidentWithoutPresenter_LogsError()
        {
            LogAssert.Expect(LogType.Error, new Regex("Resident panel"));

            CreateHost();
        }
        #endregion


        #region Private Methods
        private UIPanelHost CreateHost(params IUIPanel[] panels)
        {
            // 상주 경로는 ResourceManager를 쓰지 않으므로 넘기지 않음
            return new UIPanelHost(_root.transform, _resolver, new List<IUIPanel>(panels), null);
        }
        #endregion
    }
}