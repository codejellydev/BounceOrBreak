using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// TrampolineType과 해당 프리팹의 매핑 단위.
    /// TrampolineRegistry에서 사용됩니다.
    /// </summary>
    [Serializable]
    public class TrampolineEntry
    {
        #region Properties
        public TrampolineType Type => _type;

        public AssetReferenceGameObject Prefab => _prefab;

        /// <summary>
        /// Entry가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _prefab != null && _prefab.RuntimeKeyIsValid() == true;
        #endregion


        #region Private Variables
        [Tooltip("Trampoline 타입")]
        [SerializeField] private TrampolineType _type;

        [Tooltip("해당 타입의 Prefab AssetReference")]
        [SerializeField] private AssetReferenceGameObject _prefab;
        #endregion
    }
}