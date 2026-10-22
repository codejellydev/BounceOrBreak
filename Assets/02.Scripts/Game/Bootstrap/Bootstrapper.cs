using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Scene;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Bootstrap
{
    /// <summary>
    /// 어플리케이션의 초기화를 담당합니다.
    /// EntryScene에 배치되어 게임 실행 시 가장 먼저 실행됩니다.
    /// </summary>
    public class Bootstrapper : IAsyncStartable, IDisposable
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


        #region Properties
        /// <summary>
        /// 초기화나 첫 씬 전환에 실패했을 때 통지됩니다.
        /// </summary>
        public Observable<Unit> StartupFailed => _startupFailed;
        #endregion


        #region Private Variables
        private readonly Subject<Unit> _startupFailed = new Subject<Unit>();
        #endregion


        #region VContainer Lifecycle
        public UniTask StartAsync(CancellationToken cancellation = default)
        {
            InitializeAndTransition().Forget();
            return UniTask.CompletedTask;
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _startupFailed.Dispose();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 애플리케이션 필수 시스템을 초기화하고, 메인 게임 플로우로 진입합니다.
        /// </summary>
        private async UniTaskVoid InitializeAndTransition()
        {
            AppLogger.Info($"[Bootstrapper] Starting initialization... version: {Application.version}");

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

        private void HandleInitializationFailure()
        {
            _startupFailed.OnNext(Unit.Default);
        }
        #endregion
    }
}