using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Resource;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// 계란 생성을 담당하는 Factory 클래스.
    /// </summary>
    public class EggFactory
    {
        #region Injection
        private readonly ResourceManager _resourceManager;

        public EggFactory(ResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }
        #endregion


        #region Private Variables
        private GameObject _eggPrefab;
        #endregion


        #region Public Methods
        /// <summary>
        /// Factory를 초기화하고 계란 Prefab을 로딩합니다.
        /// </summary>
        public async UniTask InitializeAsync(CancellationToken ct)
        {
            _eggPrefab = await _resourceManager.LoadAsync<GameObject>(GameConstants.Egg.Keys.PREFAB, ct);

            if (_eggPrefab == null)
            {
                AppLogger.Error("[EggFactory] Failed to load egg prefab.");
            }
        }

        /// <summary>
        /// 계란 인스턴스를 생성합니다.
        /// </summary>
        public EggController Create()
        {
            GameObject instance = UnityEngine.Object.Instantiate(_eggPrefab);
            return instance.GetComponent<EggController>();
        }

        /// <summary>
        /// 로딩된 리소스를 해제합니다.
        /// </summary>
        public void ReleaseAll()
        {
            _resourceManager.Release(GameConstants.Egg.Keys.PREFAB, ResourceReleaseContext.Cleanup);
            _eggPrefab = null;
        }
        #endregion
    }
}