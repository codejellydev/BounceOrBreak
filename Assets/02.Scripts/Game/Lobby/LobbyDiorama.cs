using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Pause;
using CodeJelly.BounceOrBreak.Core.Resource;
using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using CodeJelly.BounceOrBreak.Game.Trampoline;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace CodeJelly.BounceOrBreak.Game.Lobby
{
    /// <summary>
    /// LobbyScene의 디오라마 클래스.
    /// </summary>
    public class LobbyDiorama : MonoBehaviour, IPauseable
    {
        #region Injection
        private ResourceManager _resourceManager;

        [Inject]
        public void Construct(ResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }
        #endregion


        #region Properties
        /// <summary>
        /// LobbyDiorama가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _config != null && _config.IsValid;
        #endregion


        #region Private Variables
        [Tooltip("디오라마의 설정")]
        [SerializeField] private LobbyDioramaConfig _config;

        private EggController _egg;
        private TrampolineBase _trampoline;
        private AssetReferenceGameObject _trampolineReference;
        #endregion


        #region Unity Lifecycle
        private void OnDestroy()
        {
            if (_resourceManager == null)
            {
                return;
            }

            if (_trampolineReference != null)
            {
                _resourceManager.Release(_trampolineReference, ResourceReleaseContext.Cleanup);
            }

            _resourceManager.Release(GameConstants.Trampoline.Keys.REGISTRY, ResourceReleaseContext.Cleanup);
            _resourceManager.Release(GameConstants.Egg.Keys.PREFAB, ResourceReleaseContext.Cleanup);
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// 프리팹을 로드해 디오라마를 만듭니다.
        /// </summary>
        public async UniTask InitializeAsync(CancellationToken ct)
        {
            if (IsValid == false)
            {
                AppLogger.Error($"[LobbyDiorama] Config is not assigned or not valid on {name}.");

                return;
            }

            TrampolineSpawnData spawnData = _config.Trampoline;

            TrampolineRegistry registry = await _resourceManager.LoadAsync<TrampolineRegistry>(GameConstants.Trampoline.Keys.REGISTRY, ct);
            TrampolineEntry entry = registry?.GetEntry(spawnData.Type);

            if (entry == null)
            {
                AppLogger.Error($"[LobbyDiorama] {spawnData.Type} Trampoline is not registered.");

                return;
            }

            _trampolineReference = entry.Prefab;

            GameObject trampolinePrefab = await _resourceManager.LoadAsync<GameObject>(_trampolineReference, ct);
            GameObject eggPrefab = await _resourceManager.LoadAsync<GameObject>(GameConstants.Egg.Keys.PREFAB, ct);

            if (trampolinePrefab == null || eggPrefab == null)
            {
                AppLogger.Error("[LobbyDiorama] Failed to load the Trampoline or Egg prefab.");

                return;
            }

            GameObject trampolineInstance = Instantiate(trampolinePrefab, transform);

            if (trampolineInstance.TryGetComponent(out _trampoline) == false)
            {
                AppLogger.Error("[LobbyDiorama] Trampoline prefab has no TrampolineBase component.");
                Destroy(trampolineInstance);

                return;
            }

            GameObject eggInstance = Instantiate(eggPrefab, transform);

            if (eggInstance.TryGetComponent(out _egg) == false)
            {
                AppLogger.Error("[LobbyDiorama] Egg prefab has no EggController component.");
                Destroy(eggInstance);

                return;
            }

            _trampoline.Initialize(null, spawnData.Config, spawnData.Points);
            _egg.Initialize(_config.Egg, _config.EggPosition);
        }

        /// <summary>
        /// Trampoline을 치웁니다. Egg가 Trampoline 아래로 내려가면 반환합니다.
        /// </summary>
        public async UniTask DropAsync(CancellationToken ct)
        {
            if (_egg == null || _trampoline == null)
            {
                return;
            }

            _trampoline.Destroy();

            Vector2[] points = _config.Trampoline.Points;
            float trampolineHeight = Mathf.Min(points[0].y, points[1].y);

            await UniTask.WaitUntil(() => _egg.transform.position.y < trampolineHeight, cancellationToken: ct);
        }

        /// <summary>
        /// IPauseable.Pause Implementation
        /// </summary>
        public void Pause()
        {
            _egg?.Pause();
        }

        /// <summary>
        /// IPauseable.Resume Implementation
        /// </summary>
        public void Resume()
        {
            _egg?.Resume();
        }
        #endregion
    }
}