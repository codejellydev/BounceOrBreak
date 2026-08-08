using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CodeJelly.BounceOrBreak.Core.Scene
{
    /// <summary>
    /// Addressables 씬에 대한 타입 안전 참조를 제공하는 래퍼 클래스.
    /// AssetReference를 사용하여 Inspector에서 드래그 앤 드롭으로 씬을 설정할 수 있습니다.
    /// </summary>
    [Serializable]
    public class SceneReference
    {
        #region Properties
        /// <summary>
        /// 전환할 타겟 씬의 Addressables 키.
        /// </summary>
        public string SceneKey
        {
            get
            {
                if (_sceneAsset == null || _sceneAsset.RuntimeKey == null)
                {
                    return string.Empty;
                }

                return _sceneAsset.RuntimeKey.ToString();
            }
        }

        /// <summary>
        /// 씬 참조가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _sceneAsset != null && _sceneAsset.RuntimeKey != null;
        #endregion


        #region Private Variables
        [Tooltip("Addressables에 등록된 Scene 에셋")]
        [SerializeField] private AssetReference _sceneAsset;
        #endregion
    }
}