using CodeJelly.BounceOrBreak.Core.Resource;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// ResourceManager의 캐시·참조 카운트·취소 처리를 검증하는 테스트 클래스.
    /// </summary>
    public class ResourceManagerTests
    {
        #region Private Variables
        private const string KEY = "Key";
        private const string OTHER_KEY = "OtherKey";

        private FakeResourceLoader _loader;
        private ResourceManager _manager;
        private TextAsset _asset;
        #endregion


        #region Public Methods
        [SetUp]
        public void SetUp()
        {
            _loader = new FakeResourceLoader();
            _manager = new ResourceManager(_loader);
            _asset = new TextAsset("asset");
        }

        [TearDown]
        public void TearDown()
        {
            _manager.Dispose();
            UnityEngine.Object.DestroyImmediate(_asset);
        }

        /// <summary>
        /// 캐시에 없는 키는 로드를 시작하고 완료된 에셋을 반환합니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_CacheMiss_ReturnsLoadedAsset()
        {
            UniTask<TextAsset> load = _manager.LoadAsync<TextAsset>(KEY);
            _loader.LastHandle.Complete(_asset);

            Assert.AreSame(_asset, await load);
            Assert.AreEqual(1, _loader.LoadCount);
        }

        /// <summary>
        /// 같은 키를 동시에 요청하면 로드는 한 번이고 둘 다 같은 에셋을 받습니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_SameKeyConcurrently_LoadsOnce()
        {
            UniTask<TextAsset> first = _manager.LoadAsync<TextAsset>(KEY);
            UniTask<TextAsset> second = _manager.LoadAsync<TextAsset>(KEY);
            _loader.LastHandle.Complete(_asset);

            Assert.AreEqual(1, _loader.LoadCount);
            Assert.AreSame(_asset, await first);
            Assert.AreSame(_asset, await second);
        }

        /// <summary>
        /// 동시 로드 중 한쪽이 취소돼도 남은 쪽은 에셋을 받고, 남은 쪽이 Release해야 해제됩니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_OneWaiterCanceled_OtherStillReceivesAsset()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            UniTask<TextAsset> canceled = _manager.LoadAsync<TextAsset>(KEY, cts.Token);
            UniTask<TextAsset> waiting = _manager.LoadAsync<TextAsset>(KEY);

            cts.Cancel();
            Assert.ThrowsAsync<OperationCanceledException>(() => canceled.AsTask());
            Assert.AreEqual(0, _loader.LastHandle.ReleaseCount);

            _loader.LastHandle.Complete(_asset);
            Assert.AreSame(_asset, await waiting);
            Assert.AreEqual(0, _loader.LastHandle.ReleaseCount);

            _manager.Release(KEY);
            Assert.AreEqual(1, _loader.LastHandle.ReleaseCount);
        }

        /// <summary>
        /// 로드가 실패하면 대기자 전부가 null을 받고 핸들은 한 번만 해제됩니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_LoadFails_AllWaitersGetNullAndHandleReleasedOnce()
        {
            UniTask<TextAsset> first = _manager.LoadAsync<TextAsset>(KEY);
            UniTask<TextAsset> second = _manager.LoadAsync<TextAsset>(KEY);
            FakeResourceLoader.FakeHandle handle = _loader.LastHandle;

            handle.Complete(null);

            Assert.IsNull(await first);
            Assert.IsNull(await second);
            Assert.AreEqual(1, handle.ReleaseCount);
        }

        /// <summary>
        /// 실패한 뒤의 요청은 새 로드를 시작합니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_AfterFailure_StartsNewLoad()
        {
            UniTask<TextAsset> failed = _manager.LoadAsync<TextAsset>(KEY);
            _loader.LastHandle.Complete(null);
            await failed;

            UniTask<TextAsset> retry = _manager.LoadAsync<TextAsset>(KEY);
            _loader.LastHandle.Complete(_asset);

            Assert.AreEqual(2, _loader.LoadCount);
            Assert.AreSame(_asset, await retry);
        }

        /// <summary>
        /// 캐시 히트에서 취소돼도 참조 카운트가 남지 않습니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_CanceledOnCacheHit_DoesNotLeakReference()
        {
            UniTask<TextAsset> load = _manager.LoadAsync<TextAsset>(KEY);
            _loader.LastHandle.Complete(_asset);
            await load;

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.ThrowsAsync<OperationCanceledException>(() => _manager.LoadAsync<TextAsset>(KEY, cts.Token).AsTask());

            _manager.Release(KEY);
            Assert.AreEqual(1, _loader.LastHandle.ReleaseCount);
        }

        /// <summary>
        /// 로드 중 Release는 대기자가 남아 있는 동안 핸들을 유지합니다.
        /// </summary>
        [Test]
        public async Task Release_WhileLoading_KeepsHandleUntilLastWaiter()
        {
            UniTask<TextAsset> first = _manager.LoadAsync<TextAsset>(KEY);
            UniTask<TextAsset> second = _manager.LoadAsync<TextAsset>(KEY);

            _manager.Release(KEY);
            Assert.AreEqual(0, _loader.LastHandle.ReleaseCount);

            _loader.LastHandle.Complete(_asset);
            Assert.AreSame(_asset, await first);
            Assert.AreSame(_asset, await second);

            _manager.Release(KEY);
            Assert.AreEqual(1, _loader.LastHandle.ReleaseCount);
        }

        /// <summary>
        /// ReleaseAll은 참조 카운트와 무관하게 모든 핸들을 해제합니다.
        /// </summary>
        [Test]
        public async Task ReleaseAll_ReleasesEveryHandle()
        {
            UniTask<TextAsset> first = _manager.LoadAsync<TextAsset>(KEY);
            UniTask<TextAsset> second = _manager.LoadAsync<TextAsset>(OTHER_KEY);
            _loader.Handles[0].Complete(_asset);
            _loader.Handles[1].Complete(_asset);
            await first;
            await second;

            _manager.ReleaseAll();

            Assert.AreEqual(1, _loader.Handles[0].ReleaseCount);
            Assert.AreEqual(1, _loader.Handles[1].ReleaseCount);
        }
        #endregion
    }
}