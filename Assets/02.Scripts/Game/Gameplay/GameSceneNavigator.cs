using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Gameplay.Pause;
using CodeJelly.BounceOrBreak.Game.UI.Navigation;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Gameplay
{
    /// <summary>
    /// GameScene의 정지 진입과 로비 복귀를 담당하는 클래스.
    /// </summary>
    public class GameSceneNavigator : IStartable, IDisposable
    {
        #region Injection
        private readonly GameFlowController _flowController;
        private readonly SceneLoader _sceneLoader;
        private readonly SceneTransitionConfig _gameSceneConfig;
        private readonly UINavigationService _navigation;

        public GameSceneNavigator(
            GameFlowController flowController,
            SceneLoader sceneLoader,
            SceneTransitionConfig gameSceneConfig,
            UINavigationService navigation)
        {
            _flowController = flowController;
            _sceneLoader = sceneLoader;
            _gameSceneConfig = gameSceneConfig;
            _navigation = navigation;
        }
        #endregion


        #region Private Variables
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        #endregion


        #region VContainer Lifecycle
        public void Start()
        {
            _navigation.PushBackHandler(OnBackPressedAtRoot)
                       .AddTo(_disposables);

            Application.focusChanged += OnFocusChanged;
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// 게임을 멈추고 정지 패널을 엽니다.
        /// </summary>
        public void RequestPause()
        {
            // 전환 중에는 곧 사라질 씬이므로 무시
            if (_sceneLoader.IsTransitioning == true)
            {
                return;
            }

            if (_flowController.Phase.CurrentValue != GamePhase.Playing
                || _flowController.IsPaused.CurrentValue == true)
            {
                return;
            }

            PauseAsync(_cts.Token).Forget();
        }

        /// <summary>
        /// 로비로 전환합니다.
        /// </summary>
        public void ReturnToLobby()
        {
            AppLogger.Info("[GameSceneNavigator] Transitioning to LobbyScene...");
            _sceneLoader.LoadSceneAsync(_gameSceneConfig).Forget();
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            Application.focusChanged -= OnFocusChanged;

            _cts.Cancel();
            _cts.Dispose();
            _disposables.Dispose();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// GameScene의 뒤로가기 기본 동작입니다. (패널이 없을 때)
        /// </summary>
        private void OnBackPressedAtRoot()
        {
            if (_flowController.Phase.CurrentValue == GamePhase.GameOver)
            {
                ReturnToLobby();

                return;
            }

            RequestPause();
        }

        /// <summary>
        /// 앱이 포커스를 잃었을 때의 동작입니다.
        /// </summary>
        private void OnFocusChanged(bool hasFocus)
        {
            if (hasFocus == true)
            {
                return;
            }

            AppLogger.Info("[GameSceneNavigator] Focus lost. Pausing.");
            RequestPause();
        }

        /// <summary>
        /// 정지 패널의 결과를 처리합니다.
        /// </summary>
        private async UniTaskVoid PauseAsync(CancellationToken ct)
        {
            _flowController.Pause();

            if (_flowController.IsPaused.CurrentValue == false)
            {
                return;
            }

            PausePanelResult result;

            try
            {
                result = await _navigation.ShowAsync<PausePanelResult>(GameConstants.UI.Keys.PAUSE, ct);
            }
            catch (OperationCanceledException)
            {
                // 씬이 먼저 내려간 것이므로 처리할 결과가 없음
                return;
            }

            if (result == PausePanelResult.Exit)
            {
                ReturnToLobby();

                return;
            }

            _flowController.Resume();
        }
        #endregion
    }
}