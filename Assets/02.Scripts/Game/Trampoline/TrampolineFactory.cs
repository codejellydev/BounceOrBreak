using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Resource;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// Trampoline 생성을 담당하는 Factory 클래스.
    /// </summary>
    public class TrampolineFactory
    {
        #region Injection
        private readonly ResourceManager _resourceManager;
        private readonly TrampolineRegistry _registry;
        private readonly Transform _root;

        public TrampolineFactory(
            ResourceManager resourceManager,
            TrampolineRegistry registry,
            Transform root)
        {
            _resourceManager = resourceManager;
            _registry = registry;
            _root = root;
        }
        #endregion


        #region Private Variables
        private Dictionary<TrampolineType, GameObject> _prefabs;
        #endregion


        #region Public Methods
        /// <summary>
        /// Factory를 초기화하고 Registry에 등록된 Prefab들을 로딩합니다.
        /// </summary>
        public async UniTask InitializeAsync(CancellationToken ct)
        {
            _prefabs = new Dictionary<TrampolineType, GameObject>();

            foreach (TrampolineEntry entry in _registry.Entries)
            {
                if (entry.IsValid == false)
                {
                    AppLogger.Warning($"[TrampolineFactory] Invalid AssetReference for type: {entry.Type}. Skipping.");
                    continue;
                }

                GameObject prefab = await _resourceManager.LoadAsync<GameObject>(entry.Prefab, ct);

                if (prefab != null)
                {
                    _prefabs[entry.Type] = prefab;
                    AppLogger.Debug($"[TrampolineFactory] Loaded prefab for type: {entry.Type}");
                }
                else
                {
                    AppLogger.Error($"[TrampolineFactory] Failed to load prefab for type: {entry.Type}");
                }
            }

            AppLogger.Info($"[TrampolineFactory] Initialized. ({_prefabs.Count}/{_registry.Entries.Count} prefabs loaded)");
        }

        /// <summary>
        /// 지정된 타입의 Trampoline을 생성합니다.
        /// </summary>
        public ITrampoline Create(TrampolineType type)
        {
            if (_prefabs.TryGetValue(type, out GameObject prefab) == true)
            {
                GameObject instance = UnityEngine.Object.Instantiate(prefab, _root);

                return instance.GetComponent<ITrampoline>();
            }
            else
            {
                throw new ArgumentException($"[TrampolineFactory] No prefab loaded for TrampolineType: {type}");
            }
        }

        /// <summary>
        /// 로딩된 리소스들을 모두 해제합니다.
        /// </summary>
        public void ReleaseAll()
        {
            foreach (TrampolineEntry entry in _registry.Entries)
            {
                if (entry.IsValid == true)
                {
                    _resourceManager.Release(entry.Prefab, ResourceReleaseContext.Cleanup);
                }
            }

            _prefabs.Clear();

            AppLogger.Debug("[TrampolineFactory] All resources released.");
        }
        #endregion
    }
}