using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Scene;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Bootstrap
{
    /// <summary>
    /// 어플리케이션의 초기화를 담당합니다.
    /// EntryScene에 배치되어 게임 실행 시 가장 먼저 실행됩니다.
    /// </summary>
    public class Bootstrapper : IAsyncStartable
    {
        #region Injection
        private readonly AppInitializer _appInitializer;
        private readonly SceneLoader _sceneLoader;
        private readonly SceneTransitionConfig _sceneConfig;

        public Bootstrapper(
            AppInitializer appInitializer,
            SceneLoader sceneLoader,
            SceneTransitionConfig sceneConfig)
        {
            _appInitializer = appInitializer;
            _sceneLoader = sceneLoader;
            _sceneConfig = sceneConfig;
        }
        #endregion


        #region VContainer Lifecycle
        public UniTask StartAsync(CancellationToken cancellation = default)
        {
            InitializeAndTransition().Forget();
            return UniTask.CompletedTask;
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 애플리케이션 필수 시스템을 초기화하고, 메인 게임 플로우로 진입합니다.
        /// </summary>
        private async UniTaskVoid InitializeAndTransition()
        {
            AppLogger.Info("[Bootstrapper] Starting initialization...");

            // 애플리케이션 필수 시스템 초기화
            if (await _appInitializer.EnsureInitializedAsync() == false)
            {
                AppLogger.Error("[Bootstrapper] Startup sequence aborted due to system initialization failure.");
                HandleInitializationFailure();
                return;
            }

            // 메인 게임 플로우로 진입
            if (await EnterGameFlowAsync() == false)
            {
                AppLogger.Error("[Bootstrapper] Startup sequence aborted due to scene transition failure.");
                HandleInitializationFailure();
                return;
            }

            // EnterGameFlowAsync 성공 시 EntryScene이 언로드되므로, 이 로그는 출력되지 않을 확률이 높음
            AppLogger.Info("[Bootstrapper] Initialization complete.");
        }

        /// <summary>
        /// 설정된 시작 씬을 로드하여 게임의 메인 플로우로 진입합니다.
        /// </summary>
        private async UniTask<bool> EnterGameFlowAsync()
        {
            if (_sceneConfig == null)
            {
                AppLogger.Error("[Bootstrapper] SceneTransitionConfig is not assigned.");
                return false;
            }

            AppLogger.Info($"[Bootstrapper] Transitioning to {_sceneConfig.TargetSceneKey}...");

            try
            {
                await _sceneLoader.LoadSceneAsync(_sceneConfig);
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Exception(ex, "[Bootstrapper] Scene transition failed.");
                return false;
            }
        }

        /// <summary>
        /// 초기화 실패 시 처리를 수행합니다.
        /// TODO: 에러 UI 구현 필요
        /// </summary>
        private void HandleInitializationFailure()
        {

        }
        #endregion
    }
}