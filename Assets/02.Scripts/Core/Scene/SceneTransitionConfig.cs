using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Core.Scene
{
    /// <summary>
    /// 씬 전환 시 필요한 설정을 관리하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "SceneTransitionConfig", menuName = "Configs/Core/Scene Transition Config")]
    public class SceneTransitionConfig : ScriptableObject
    {
        #region Properties
        public string TargetSceneKey => _targetScene.SceneKey;
        public bool UseLoadingScene => _useLoadingScene;
        public string LoadingSceneKey => _loadingScene.SceneKey;
        #endregion


        #region Private Variables
        [Header("Target Scene")]
        [Tooltip("전환할 타겟 Scene")]
        [SerializeField] private SceneReference _targetScene;

        [Header("Loading Scene")]
        [Tooltip("Scene 전환 시 Loading Scene 사용 여부")]
        [SerializeField] private bool _useLoadingScene = true;

        [SerializeField] private SceneReference _loadingScene;
        #endregion


        #region Unity Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_targetScene != null && _targetScene.IsValid == false)
            {
                AppLogger.Warning($"[SceneTransitionConfig] Target scene is not set in {name}");
            }

            if (_useLoadingScene == true && (_loadingScene == null || _loadingScene.IsValid == false))
            {
                AppLogger.Warning($"[SceneTransitionConfig] Loading scene is enabled but not set in {name}");
            }
        }
#endif
        #endregion
    }
}