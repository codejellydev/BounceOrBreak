using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Pause;
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
    public class EggSpawner : IDisposable, IPauseable
    {
        #region Injection
        private readonly ResourceManager _resourceManager;
        private readonly RoundProgress _roundProgress;

        public EggSpawner(ResourceManager resourceManager, RoundProgress roundProgress)
        {
            _resourceManager = resourceManager;
            _roundProgress = roundProgress;
        }
        #endregion


        #region Private Variables
        /// <summary>
        /// Pool에서 꺼내져 활성 상태인 Egg 집합.
        /// </summary>
        private readonly HashSet<IEgg> _activeEggs = new HashSet<IEgg>();
        private readonly PausableStopwatch _stopwatch = new PausableStopwatch();

        private EggFactory _factory;
        private EggRegistry _registry;
        private IObjectPool<IEgg> _pool;

        private EggSpawnConfig _config;
        private WaveProgression _progression;

        private CancellationTokenSource _spawnCts;
        private IEggTypeSelector _selector;

        /// <summary>
        /// Selector를 마지막으로 만든 Wave.
        /// </summary>
        private WaveData _selectorWave;
        #endregion


        #region Public Methods
        /// <summary>
        /// 스폰 시스템을 초기화합니다.
        /// </summary>
        /// <param name="config">진행에 사용할 스폰 설정</param>
        public async UniTask InitializeAsync(EggSpawnConfig config, CancellationToken ct)
        {
            _config = config;

            if (_config == null)
            {
                AppLogger.Error("[EggSpawner] EggSpawnConfig is null. Eggs will not spawn.");
            }
            else
            {
                _progression = _config.Progression;
            }

            _registry = await _resourceManager.LoadAsync<EggRegistry>(GameConstants.Egg.Keys.REGISTRY, ct);

            _factory = new EggFactory(_resourceManager);
            await _factory.InitializeAsync(ct);

            InitializePool();

            AppLogger.Info("[EggSpawner] Initialized.");
        }

        /// <summary>
        /// 진행을 초기화하고 새 Round의 스폰을 시작합니다.
        /// </summary>
        public void StartNewRound()
        {
            StopSpawning();
            DespawnAll();

            _roundProgress.Reset();
            _selectorWave = null;

            if (HasValidWaves() == false)
            {
                return;
            }

            _factory.PrepareRoot();

            _spawnCts = new CancellationTokenSource();
            RunSpawnLoop(_spawnCts.Token).Forget();
        }

        /// <summary>
        /// 스폰 루프를 중단합니다.
        /// </summary>
        public void StopSpawning()
        {
            _spawnCts?.Cancel();
            _spawnCts?.Dispose();
            _spawnCts = null;
        }

        /// <summary>
        /// Egg를 Pool에 반납합니다.
        /// </summary>
        /// <param name="egg">반납할 Egg</param>
        public void Despawn(IEgg egg)
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
        /// 활성 상태인 모든 Egg를 Pool에 반납합니다.
        /// </summary>
        public void DespawnAll()
        {
            // Despawn은 순회 중 컬렉션을 수정하므로 Pool에 직접 반납함
            foreach (IEgg egg in _activeEggs)
            {
                _pool.Release(egg);
            }

            _activeEggs.Clear();
        }

        /// <summary>
        /// 지정된 EggColor를 가진 Egg가 어느 Wave에서든 등장하는지 여부를 반환합니다.
        /// </summary>
        public bool CanSpawnColor(EggColor color)
        {
            if (HasAnyWave() == false)
            {
                return false;
            }

            // 어느 Wave에서든 등장하면 스폰 가능으로 판단
            foreach (WaveData wave in _progression.Waves)
            {
                foreach (EggType type in wave.Types)
                {
                    EggData data = _registry.GetData(type);

                    if (data != null && data.Color == color)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// IPauseable.Pause Implementation
        /// </summary>
        public void Pause()
        {
            _stopwatch.Pause();

            foreach (IEgg egg in _activeEggs)
            {
                egg.Pause();
            }
        }

        /// <summary>
        /// IPauseable.Resume Implementation
        /// </summary>
        public void Resume()
        {
            _stopwatch.Resume();

            foreach (IEgg egg in _activeEggs)
            {
                egg.Resume();
            }
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            StopSpawning();

            // Clear는 Pool에 있는 Egg만 파괴하므로, 먼저 회수해야 모두 Destroy를 거침
            DespawnAll();

            _pool?.Clear();
            _factory?.ReleaseAll();

            _resourceManager.Release(GameConstants.Egg.Keys.REGISTRY, ResourceReleaseContext.Cleanup);

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
        /// 모든 Wave가 스폰 가능한 구성인지 확인합니다.
        /// <para>Wave가 하나도 없거나 EggType이 비어 있으면 유효하지 않습니다.</para>
        /// </summary>
        private bool HasValidWaves()
        {
            if (HasAnyWave() == false)
            {
                AppLogger.Error("[EggSpawner] No wave is defined. Spawning is skipped.");

                return false;
            }

            for (int i = 0; i < _progression.WaveCount; i++)
            {
                if (_progression.Waves[i].Types.Count == 0)
                {
                    AppLogger.Error($"[EggSpawner] Wave {i} has no EggType. Spawning is skipped.");

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 진행에 사용할 Wave가 하나라도 정의되어 있는지 여부를 반환합니다.
        /// </summary>
        private bool HasAnyWave()
        {
            return _config != null && 0 < _progression.WaveCount;
        }

        /// <summary>
        /// 스폰 간격마다 Egg를 스폰합니다.
        /// </summary>
        private async UniTaskVoid RunSpawnLoop(CancellationToken ct)
        {
            while (ct.IsCancellationRequested == false)
            {
                int spawnedCount = _roundProgress.SpawnedCount.CurrentValue;
                float delay = (spawnedCount == 0)
                    ? _config.InitialSpawnDelay
                    : _progression.GetSpawnInterval(spawnedCount);

                await _stopwatch.WaitForSecondsAsync(delay, ct);
                SpawnNext();
            }
        }

        /// <summary>
        /// 다음 Egg를 Pool에서 꺼내 스폰합니다.
        /// </summary>
        private void SpawnNext()
        {
            WaveData wave = _progression.GetWave(_roundProgress.SpawnedCount.CurrentValue);
            IEggTypeSelector selector = EnsureSelector(wave);

            EggType type = selector.Select(wave);
            EggData data = _registry.GetData(type);

            if (data == null)
            {
                AppLogger.Error($"[EggSpawner] {type} is not registered in EggRegistry.");

                return;
            }

            IEgg egg = _pool.Get();

            if (egg.Initialize(data, _config.SpawnPosition) == false)
            {
                AppLogger.Error("[EggSpawner] Failed to initialize egg.");
                _pool.Release(egg);

                return;
            }

            _activeEggs.Add(egg);
            _roundProgress.CountSpawn();
        }

        /// <summary>
        /// 지정된 Wave의 Selector를 반환합니다.
        /// Wave가 바뀌면 이전 Wave의 선택 상태가 이어지지 않도록 새로 만듭니다.
        /// </summary>
        private IEggTypeSelector EnsureSelector(WaveData wave)
        {
            if (_selectorWave != wave)
            {
                _selector = EggTypeSelectorFactory.Create(wave.SelectionPolicy);
                _selectorWave = wave;
            }

            return _selector;
        }
        #endregion
    }
}
