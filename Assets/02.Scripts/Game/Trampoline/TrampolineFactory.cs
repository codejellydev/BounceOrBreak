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
        #region Private Variables
        private Dictionary<TrampolineType, GameObject> _prefabs;
        #endregion


        #region Public Methods
        /// <summary>
        /// Factory를 초기화하고 Prefab들을 로딩합니다.
        /// </summary>
        public async UniTask InitializeAsync(CancellationToken ct)
        {
            _prefabs = new Dictionary<TrampolineType, GameObject>();

            var straight = await ResourceManager.Instance.LoadAsync<GameObject>(GameConstants.Trampoline.Keys.STRAIGHT_PREFAB, ct);
            _prefabs[TrampolineType.Straight] = straight;

            var freeform = await ResourceManager.Instance.LoadAsync<GameObject>(GameConstants.Trampoline.Keys.FREEFORM_PREFAB, ct);
            _prefabs[TrampolineType.Freeform] = freeform;
        }

        /// <summary>
        /// 지정된 타입의 Trampoline을 생성합니다.
        /// </summary>
        public ITrampoline Create(TrampolineType type)
        {
            if (_prefabs.TryGetValue(type, out GameObject prefab) == true)
            {
                GameObject instance = UnityEngine.Object.Instantiate(prefab);

                return instance.GetComponent<ITrampoline>();
            }
            else
            {
                throw new ArgumentException($"Unknown TrampolineType: {type}");
            }
        }

        /// <summary>
        /// 로딩된 리소스들을 모두 해제합니다.
        /// </summary>
        public void ReleaseAll()
        {
            ResourceManager.Instance.Release(GameConstants.Trampoline.Keys.STRAIGHT_PREFAB, ResourceReleaseContext.Cleanup);
            ResourceManager.Instance.Release(GameConstants.Trampoline.Keys.FREEFORM_PREFAB, ResourceReleaseContext.Cleanup);

            _prefabs.Clear();
        }
        #endregion
    }
}