using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Resource;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// Egg 생성을 담당하는 Factory 클래스.
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

        /// <summary>
        /// 생성된 Egg들이 배치될 루트.
        /// </summary>
        private Transform _root;
        #endregion


        #region Public Methods
        /// <summary>
        /// Factory를 초기화하고 Egg 프리팹을 로딩합니다.
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
        /// 생성된 Egg들이 배치될 루트를 준비합니다.
        /// </summary>
        public void PrepareRoot()
        {
            if (_root != null)
            {
                return;
            }

            _root = new GameObject(GameConstants.Egg.ROOT_NAME).transform;
        }

        /// <summary>
        /// Egg 인스턴스를 생성합니다.
        /// </summary>
        public EggController Create()
        {
            if (_root == null)
            {
                AppLogger.Error("[EggFactory] Root is not prepared. Call PrepareRoot after scene activation.");

                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(_eggPrefab, _root);
            return instance.GetComponent<EggController>();
        }

        /// <summary>
        /// 로딩된 리소스를 해제합니다.
        /// </summary>
        public void ReleaseAll()
        {
            _resourceManager.Release(GameConstants.Egg.Keys.PREFAB, ResourceReleaseContext.Cleanup);
            _eggPrefab = null;

            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root.gameObject);
                _root = null;
            }
        }
        #endregion
    }
}