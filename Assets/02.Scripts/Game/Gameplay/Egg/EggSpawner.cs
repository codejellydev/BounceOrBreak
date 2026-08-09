using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Pooling;
using CodeJelly.BounceOrBreak.Core.Resource;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Pool;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// 스폰 로직에 따라 Egg를 생성하는 클래스.
    /// </summary>
    public class EggSpawner : IDisposable
    {
        #region Injection
        private readonly ResourceManager _resourceManager;

        public EggSpawner(ResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }
        #endregion


        #region Private Variables
        /// <summary>
        /// 풀에서 꺼내져 활성 상태인 Egg 집합.
        /// </summary>
        private readonly HashSet<EggController> _activeEggs = new HashSet<EggController>();

        private EggFactory _factory;
        private EggRegistry _registry;
        private EggSpawnConfig _config;

        private IObjectPool<EggController> _pool;
        private CancellationTokenSource _spawnCts;
        #endregion


        #region Public Methods
        /// <summary>
        /// 스폰 시스템을 초기화합니다.
        /// </summary>
        public async UniTask InitializeAsync(CancellationToken ct)
        {
            _registry = await _resourceManager.LoadAsync<EggRegistry>(GameConstants.Egg.Keys.REGISTRY, ct);
            _config = await _resourceManager.LoadAsync<EggSpawnConfig>(GameConstants.Egg.Keys.SPAWN_CONFIG, ct);

            _factory = new EggFactory(_resourceManager);
            await _factory.InitializeAsync(ct);

            InitializePool();

            AppLogger.Info("[EggSpawner] Initialized.");
        }

        /// <summary>
        /// 스폰 루프를 시작합니다.
        /// </summary>
        public void StartSpawning()
        {
            StopSpawning();
            DespawnAll();

            _factory.PrepareRoot();

            _spawnCts = new CancellationTokenSource();
            RunSpawnLoop(_spawnCts.Token).Forget();
        }

        /// <summary>
        /// 스폰 루프를 정지합니다.
        /// </summary>
        public void StopSpawning()
        {
            _spawnCts?.Cancel();
            _spawnCts?.Dispose();
            _spawnCts = null;
        }

        /// <summary>
        /// Egg를 풀에 반납합니다.
        /// </summary>
        /// <param name="egg">반납할 Egg</param>
        public void Despawn(EggController egg)
        {
            if (egg == null)
            {
                AppLogger.Error("[EggSpawner] Cannot despawn a null egg.");

                return;
            }

            _activeEggs.Remove(egg);
            _pool.Release(egg);
        }

        /// <summary>
        /// 활성 상태인 모든 Egg를 풀에 반납합니다.
        /// </summary>
        public void DespawnAll()
        {
            // Despawn은 순회 중 컬렉션을 수정하므로 풀에 직접 반납함
            foreach (EggController egg in _activeEggs)
            {
                _pool.Release(egg);
            }

            _activeEggs.Clear();
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            StopSpawning();

            // Clear는 풀에 있는 Egg만 파괴하므로, 먼저 회수해야 모두 Destroy를 거침
            DespawnAll();

            _pool?.Clear();
            _factory?.ReleaseAll();

            _resourceManager.Release(GameConstants.Egg.Keys.REGISTRY, ResourceReleaseContext.Cleanup);
            _resourceManager.Release(GameConstants.Egg.Keys.SPAWN_CONFIG, ResourceReleaseContext.Cleanup);

            AppLogger.Info("[EggSpawner] Disposed cleanly.");
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// ObjectPool을 초기화합니다.
        /// </summary>
        private void InitializePool()
        {
            _pool = PoolExtensions.CreatePoolablePool
            (
                createFunc: _factory.Create,
                defaultCapacity: GameConstants.Egg.POOL_DEFAULT_CAPACITY,
                maxSize: GameConstants.Egg.POOL_MAX_SIZE
            );
        }

        /// <summary>
        /// 스폰 간격마다 Egg를 스폰합니다.
        /// </summary>
        private async UniTaskVoid RunSpawnLoop(CancellationToken ct)
        {
            while (ct.IsCancellationRequested == false)
            {
                await UniTask.WaitForSeconds(_config.SpawnInterval, cancellationToken: ct);
                SpawnNext();
            }
        }

        /// <summary>
        /// 다음 Egg를 풀에서 꺼내 스폰합니다.
        /// </summary>
        private void SpawnNext()
        {
            EggController egg = _pool.Get();

            EggType type = SelectNextType();
            EggTypeData data = _registry.GetData(type);

            egg.transform.position = _config.SpawnPosition;

            if (egg.Initialize(data) == false)
            {
                AppLogger.Error($"[EggSpawner] Failed to initialize egg. Type: {type}.");
                _pool.Release(egg);

                return;
            }

            _activeEggs.Add(egg);
        }

        /// <summary>
        /// 다음에 스폰할 Egg 종류를 결정합니다.
        /// </summary>
        private EggType SelectNextType()
        {
            // TODO: 웨이브 기반 선택 로직
            return EggType.Default;
        }
        #endregion
    }
}