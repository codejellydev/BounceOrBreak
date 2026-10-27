using CodeJelly.BounceOrBreak.Core.Logging;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.AddressableAssets;

namespace CodeJelly.BounceOrBreak.Core.Resource
{
    /// <summary>
    /// 리소스를 캐시하고 참조 카운트로 수명을 관리하는 클래스.
    /// </summary>
    public class ResourceManager : IDisposable
    {
        #region Injection
        private readonly IResourceLoader _loader;

        public ResourceManager(IResourceLoader loader)
        {
            _loader = loader;
        }
        #endregion


        #region Properties
        /// <summary>
        /// 현재 로딩 중인 리소스 개수.
        /// </summary>
        public ReadOnlyReactiveProperty<int> LoadingCount => _loadingCount.ToReadOnlyReactiveProperty();
        #endregion


        #region Private Variables
        private readonly Dictionary<string, ResourceNode> _resourceCache =
            new Dictionary<string, ResourceNode>();

        private readonly ReactiveProperty<int> _loadingCount = new ReactiveProperty<int>(0);
        #endregion


        #region Public Methods
        /// <summary>
        /// 리소스 시스템을 초기화합니다.
        /// </summary>
        public UniTask<bool> InitializeAsync()
        {
            return _loader.InitializeAsync();
        }

        /// <summary>
        /// 리소스를 비동기로 로드합니다.
        /// 이미 로드된 리소스는 캐시에서 반환하며, 참조 카운트를 증가시킵니다.
        /// <para>
        /// 취소 토큰 전달 시 호출부에서 try-catch로 OperationCanceledException 처리 필요.
        /// 취소 로직이 불필요한 경우 CancellationToken 생략 가능.
        /// </para>
        /// </summary>
        public async UniTask<T> LoadAsync<T>(string key, CancellationToken ct = default) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(key) == true)
            {
                AppLogger.Error("[ResourceManager] Key is null or empty.");
                return null;
            }

            ResourceNode node = Acquire<T>(key);

            try
            {
                await node.Task.AttachExternalCancellation(ct);
            }
            catch (OperationCanceledException)
            {
                AppLogger.Info($"[ResourceManager] Load Canceled: {key}");
                ReleaseNode(node);
                throw; // 취소 exception은 상위 호출 스택으로 전파
            }

            if (node.IsFailed == true)
            {
                ReleaseNode(node);
                return null;
            }

            return node.GetAsset<T>();
        }

        /// <summary>
        /// AssetReference를 통해 리소스를 비동기로 로드합니다.
        /// <para>
        /// 취소 토큰 전달 시 호출부에서 try-catch로 OperationCanceledException 처리 필요.
        /// 취소 로직이 불필요한 경우 CancellationToken 생략 가능.
        /// </para>
        /// </summary>
        public async UniTask<T> LoadAsync<T>(AssetReference reference, CancellationToken ct = default) where T : UnityEngine.Object
        {
            string key = ResolveKey(reference);

            if (key == null)
            {
                AppLogger.Error("[ResourceManager] AssetReference is null or has invalid RuntimeKey.");
                return null;
            }

            return await LoadAsync<T>(key, ct);
        }

        /// <summary>
        /// 리소스의 참조 카운트를 감소시키고, 0이 되면 메모리에서 해제합니다.
        /// </summary>
        public void Release(string key, ResourceReleaseContext context = ResourceReleaseContext.Normal)
        {
            if (string.IsNullOrEmpty(key) == true)
            {
                return;
            }

            if (_resourceCache.TryGetValue(key, out ResourceNode node) == true && node.IsFailed == false)
            {
                ReleaseNode(node);
            }
            else
            {
                if (context == ResourceReleaseContext.Normal)
                {
                    AppLogger.Warning($"[ResourceManager] Resource not in cache: {key} (Already released, wrong key, or load failed)");
                }
                else // Cleanup
                {
                    AppLogger.Debug($"[ResourceManager] Resource not in cache: {key} (Already released or never loaded - normal during cleanup)");
                }
            }
        }

        /// <summary>
        /// AssetReference에 해당하는 리소스의 참조 카운트를 감소시키고, 0이 되면 메모리에서 해제합니다.
        /// </summary>
        public void Release(AssetReference reference, ResourceReleaseContext context = ResourceReleaseContext.Normal)
        {
            string key = ResolveKey(reference);

            if (key == null)
            {
                return;
            }

            Release(key, context);
        }

        /// <summary>
        /// 모든 리소스를 강제로 해제합니다.
        /// <para>참조 카운트와 무관하게 즉시 해제되므로 주의 필요.</para>
        /// </summary>
        public void ReleaseAll()
        {
            int count = _resourceCache.Count;
            foreach (ResourceNode node in _resourceCache.Values)
            {
                node.Release();
            }

            _resourceCache.Clear();
            _loadingCount.Value = 0;

            AppLogger.Info($"[ResourceManager] ReleaseAll Completed. ({count} items)");
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            ReleaseAll();
            _loadingCount.Dispose();

            AppLogger.Info("[ResourceManager] Disposed cleanly.");
        }
        #endregion


        #region Private Methods
        private ResourceNode Acquire<T>(string key) where T : UnityEngine.Object
        {
            if (_resourceCache.TryGetValue(key, out ResourceNode node) == true)
            {
                if (node.IsFailed == false)
                {
                    node.IncrementReference();
                    return node;
                }

                _resourceCache.Remove(key);
            }

            node = new ResourceNode(_loader.Load<T>(key), key);
            _resourceCache.Add(key, node);
            TrackLoading(node);

            return node;
        }

        private void TrackLoading(ResourceNode node)
        {
            _loadingCount.Value++;

            node.Task.ContinueWith(() =>
            {
                // 에디터가 플레이 모드를 나갈 때 UniTask가 남은 continuation을 flush하므로 Dispose 뒤에도 도달함
                if (_loadingCount.IsDisposed == false)
                {
                    _loadingCount.Value--;
                }
            }).Forget();
        }

        private void ReleaseNode(ResourceNode node)
        {
            node.DecrementReference();

            if (node.ReferenceCount > 0)
            {
                AppLogger.Debug($"[ResourceManager] Decremented RefCount: {node.Key} (Remaining: {node.ReferenceCount})");
                return;
            }

            if (_resourceCache.TryGetValue(node.Key, out ResourceNode cached) == true && cached == node)
            {
                _resourceCache.Remove(node.Key);
            }

            node.Release();

            AppLogger.Debug($"[ResourceManager] Released: {node.Key} (RefCount reached 0)");
        }

        /// <summary>
        /// AssetReference로부터 캐시 key로 사용할 문자열을 추출합니다.
        /// </summary>
        /// <returns>유효한 키 문자열. 유효하지 않으면 null.</returns>
        private string ResolveKey(AssetReference reference)
        {
            if (reference == null || reference.RuntimeKeyIsValid() == false)
            {
                return null;
            }

            return reference.RuntimeKey.ToString();
        }
        #endregion
    }
}