using CodeJelly.BounceOrBreak.Core.Logging;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CodeJelly.BounceOrBreak.Core.Resource
{
    /// <summary>
    /// Addressables 기반의 리소스 관리 클래스.
    /// </summary>
    public class ResourceManager : IDisposable
    {
        ResourceManager()
        {
            _loadingCount = new ReactiveProperty<int>(0);

            _resourceNodePool = new ObjectPool<ResourceNode>(
                createFunc: () => new ResourceNode(),
                actionOnGet: node => node.Get(),
                actionOnRelease: node => node.Release(),
                actionOnDestroy: node => node.Destroy(),
                collectionCheck: true, // 이미 Release된 오브젝트를 다시 Release 시도하는 경우 exception 발생
                defaultCapacity: AppConstants.Resource.POOL_DEFAULT_CAPACITY,
                maxSize: AppConstants.Resource.POOL_MAX_POOL_SIZE
            );

            Application.quitting += Dispose;
        }


        #region Properties
        public static ResourceManager Instance => _instance.Value;

        /// <summary>
        /// 현재 로딩 중인 리소스 개수.
        /// </summary>
        public ReadOnlyReactiveProperty<int> LoadingCount => _loadingCount.ToReadOnlyReactiveProperty();
        #endregion


        #region Private Variables
        private static readonly Lazy<ResourceManager> _instance =
            new Lazy<ResourceManager>(() => new ResourceManager());

        private readonly Dictionary<string, ResourceNode> _resourceCache =
            new Dictionary<string, ResourceNode>();

        private readonly IObjectPool<ResourceNode> _resourceNodePool;

        private readonly ReactiveProperty<int> _loadingCount;
        #endregion


        #region Public Methods
        /// <summary>
        /// Addressables 리소스를 비동기로 로드합니다.
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

            if (_resourceCache.TryGetValue(key, out ResourceNode node) == true)
            {
                if (node.Handle.IsValid() == true)
                {
                    node.IncrementReference();

                    await node.Handle.ToUniTask(cancellationToken: ct);

                    return node.GetAsset<T>();
                }
                else
                {
                    AppLogger.Warning($"[ResourceManager] Invalid Handle found for key: {key}. Reloading...");
                    ForceRelease(key);
                }
            }

            _loadingCount.Value++;

            var handle = Addressables.LoadAssetAsync<T>(key);

            node = _resourceNodePool.Get();
            node.Initialize(handle, key);
            _resourceCache.Add(key, node);

            try
            {
                await handle.ToUniTask(cancellationToken: ct);

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return node.GetAsset<T>();
                }
                else
                {
                    AppLogger.Error($"[ResourceManager] Load Failed: {key} (Status: {handle.Status})");
                    ForceRelease(key);
                    return null;
                }
            }
            catch (OperationCanceledException)
            {
                AppLogger.Info($"[ResourceManager] Load Canceled: {key}");
                ForceRelease(key);
                throw; // 취소 exception은 상위 호출 스택으로 전파
            }
            catch (Exception ex)
            {
                AppLogger.Exception(ex);
                ForceRelease(key);
                return null;
            }
            finally
            {
                _loadingCount.Value--;
            }
        }

        /// <summary>
        /// 리소스의 참조 카운트를 감소시키고, 0이 되면 메모리에서 해제합니다.
        /// </summary>
        public void Release(string key)
        {
            if (string.IsNullOrEmpty(key) == true)
            {
                return;
            }

            if (_resourceCache.TryGetValue(key, out ResourceNode node) == true)
            {
                node.DecrementReference();

                if (node.ReferenceCount <= 0)
                {
                    if (node.Handle.IsValid() == true)
                    {
                        Addressables.Release(node.Handle);
                    }

                    _resourceCache.Remove(key);
                    _resourceNodePool.Release(node);
                }
            }
            else
            {
                AppLogger.Warning($"[ResourceManager] Try to release unknown key: {key}");
            }
        }

        /// <summary>
        /// 모든 리소스를 강제로 해제합니다.
        /// <para>참조 카운트와 무관하게 즉시 해제되므로 주의 필요.</para>
        /// </summary>
        public void ReleaseAll()
        {
            int count = _resourceCache.Count;
            foreach (var resource in _resourceCache.Values)
            {
                if (resource.Handle.IsValid() == true)
                {
                    Addressables.Release(resource.Handle);
                }
                _resourceNodePool.Release(resource);
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
            Application.quitting -= Dispose;

            ReleaseAll();
            _resourceNodePool.Clear();
            _loadingCount.Dispose();

            AppLogger.Info("[ResourceManager] Disposed cleanly.");
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 참조 카운트를 무시하고 리소스를 즉시 해제합니다.
        /// </summary>
        private void ForceRelease(string key)
        {
            if (_resourceCache.TryGetValue(key, out ResourceNode node) == true)
            {
                _resourceCache.Remove(key);

                if (node.Handle.IsValid() == true)
                {
                    Addressables.Release(node.Handle);
                }
                _resourceNodePool.Release(node);
            }
        }
        #endregion
    }
}