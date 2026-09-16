using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Resource;
using CodeJelly.BounceOrBreak.Game.UI.Panel;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.UI.Navigation
{
    /// <summary>
    /// UIRoot 단위의 패널 인스턴스 관리자.
    /// 수명 정책에 따라 인스턴스를 내어주고 회수하며, 루트와 함께 정리됩니다.
    /// </summary>
    public class UIPanelHost : IDisposable
    {
        #region Constructors
        public UIPanelHost(
            Transform root,
            IObjectResolver resolver,
            IReadOnlyList<IUIPanel> panels,
            ResourceManager resourceManager)
        {
            _root = root;
            _resolver = resolver;
            _resourceManager = resourceManager;
            _panels = new Dictionary<string, IUIPanel>();

            foreach (IUIPanel panel in panels)
            {
                if (_panels.ContainsKey(panel.Key) == true)
                {
                    AppLogger.Error($"[UIPanelHost] Duplicate panel key '{panel.Key}' under {root.name}. Later one is ignored.");

                    continue;
                }

                _panels.Add(panel.Key, panel);
            }
        }
        #endregion


        #region Properties
        /// <summary>
        /// 루트가 해제될 때 취소되는 토큰
        /// </summary>
        public CancellationToken LifetimeToken => _lifetimeCts.Token;
        #endregion


        #region Private Variables
        private readonly Transform _root;
        private readonly IObjectResolver _resolver;
        private readonly ResourceManager _resourceManager;
        private readonly Dictionary<string, IUIPanel> _panels;
        private readonly Dictionary<string, AsyncLazy<GameObject>> _cached = new Dictionary<string, AsyncLazy<GameObject>>();
        private readonly List<UIPanelConfig.Entry> _loadedEntries = new List<UIPanelConfig.Entry>();
        private readonly CancellationTokenSource _lifetimeCts = new CancellationTokenSource();
        #endregion


        #region Public Methods
        /// <summary>
        /// 키에 등록된 Presenter를 결과 타입에 맞춰 반환합니다.
        /// </summary>
        public IUIPanel<TResult> GetPanel<TResult>(string key)
        {
            if (_panels.TryGetValue(key, out IUIPanel panel) == false)
            {
                throw new InvalidOperationException($"[UIPanelHost] No panel is registered for '{key}' under {_root.name}.");
            }

            IUIPanel<TResult> typed = panel as IUIPanel<TResult>;

            if (typed == null)
            {
                throw new InvalidOperationException($"[UIPanelHost] Panel '{key}' does not return {typeof(TResult).Name}.");
            }

            return typed;
        }

        /// <summary>
        /// 수명 정책에 따라 인스턴스를 가져옵니다. 비활성 상태로 반환됩니다.
        /// </summary>
        public UniTask<GameObject> AcquireAsync(UIPanelConfig.Entry entry, CancellationToken ct)
        {
            // Transient는 열 때마다 새로 만들고 닫으면 파괴하므로 캐시하지 않음
            if (entry.Lifetime == UIPanelLifetime.Transient)
            {
                return InstantiateAsync(entry, ct);
            }

            if (_cached.TryGetValue(entry.Key, out AsyncLazy<GameObject> lazy) == false)
            {
                // 동시 요청이 겹쳐도 인스턴스는 하나여야 하므로 Lazy 공유
                lazy = UniTask.Lazy(() => InstantiateAsync(entry, _lifetimeCts.Token));
                _cached.Add(entry.Key, lazy);
            }

            return lazy.Task.AttachExternalCancellation(ct);
        }

        /// <summary>
        /// 닫힌 인스턴스를 정책대로 정리합니다.
        /// </summary>
        public void Release(UIPanelConfig.Entry entry, GameObject instance)
        {
            // 씬과 함께 이미 파괴된 인스턴스는 Dispose가 정리하므로 여기선 무시
            if (instance == null)
            {
                return;
            }

            if (entry.Lifetime != UIPanelLifetime.Transient)
            {
                instance.SetActive(false);

                return;
            }

            UnityEngine.Object.Destroy(instance);
            _loadedEntries.Remove(entry);
            _resourceManager.Release(entry.Prefab);
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();

            // 인스턴스는 씬과 함께 파괴되므로 프리팹 참조만 정리
            foreach (UIPanelConfig.Entry entry in _loadedEntries)
            {
                _resourceManager.Release(entry.Prefab, ResourceReleaseContext.Cleanup);
            }

            _loadedEntries.Clear();
            _cached.Clear();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 프리팹을 로드해 루트 밑에 비활성 인스턴스를 만듭니다.
        /// </summary>
        private async UniTask<GameObject> InstantiateAsync(UIPanelConfig.Entry entry, CancellationToken ct)
        {
            GameObject prefab = await _resourceManager.LoadAsync<GameObject>(entry.Prefab, ct);

            if (prefab == null)
            {
                throw new InvalidOperationException($"[UIPanelHost] Failed to load prefab for '{entry.Key}'.");
            }

            _loadedEntries.Add(entry);

            GameObject instance = _resolver.Instantiate(prefab, _root);
            instance.SetActive(false);

            return instance;
        }
        #endregion
    }
}