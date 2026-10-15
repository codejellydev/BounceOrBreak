using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Input;
using CodeJelly.BounceOrBreak.Game.UI.Message;
using CodeJelly.BounceOrBreak.Game.UI.Navigation;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Lobby
{
    /// <summary>
    /// LobbyScene의 Presenter 클래스.
    /// </summary>
    public class LobbyPresenter : IStartable, ISceneActivatable, IDisposable
    {
        #region Injection
        private readonly LobbyView _view;
        private readonly LobbyDiorama _diorama;
        private readonly SceneLoader _sceneLoader;
        private readonly SceneTransitionConfig _lobbySceneConfig;
        private readonly InputManager _inputManager;
        private readonly UINavigationService _navigation;

        public LobbyPresenter(
            LobbyView view,
            LobbyDiorama diorama,
            SceneLoader sceneLoader,
            SceneTransitionConfig lobbySceneConfig,
            InputManager inputManager,
            UINavigationService navigation)
        {
            _view = view;
            _diorama = diorama;
            _sceneLoader = sceneLoader;
            _lobbySceneConfig = lobbySceneConfig;
            _inputManager = inputManager;
            _navigation = navigation;
        }
        #endregion


        #region Private Variables
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private bool _isStarting;
        #endregion


        #region VContainer Lifecycle
        public void Start()
        {
            InitializeDiorama(_cts.Token).Forget();
            BindView();

            _navigation.PushBackHandler(OnBackPressedAtRoot)
                       .AddTo(_disposables);
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// ISceneActivatable.OnActivated Implementation
        /// </summary>
        public void OnActivated()
        {
            _inputManager.SwitchContext(InputContext.Lobby);
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _disposables.Dispose();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 앱을 종료합니다.
        /// </summary>
        private static void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// View와 Presenter 간의 이벤트 바인딩을 설정합니다.
        /// </summary>
        private void BindView()
        {
            _view.StartButton
                 .OnClickAsObservable()
                 .Subscribe(_ => OnStartButtonClicked())
                 .AddTo(_disposables);

            _view.SettingsButton
                 .OnClick
                 .Subscribe(_ => OnSettingsButtonClicked())
                 .AddTo(_disposables);
        }

        /// <summary>
        /// Start 버튼 클릭 이벤트를 처리합니다.
        /// </summary>
        private void OnStartButtonClicked()
        {
            if (_isStarting == true)
            {
                return;
            }

            AppLogger.Info("[LobbyPresenter] Start button clicked. Transitioning to GameScene...");
            StartGame(_cts.Token).Forget();
        }

        /// <summary>
        /// Settings 버튼 클릭 이벤트를 처리합니다.
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            AppLogger.Info("[LobbyPresenter] Settings button clicked.");
            OpenSettings(_cts.Token).Forget();
        }

        /// <summary>
        /// 로비의 뒤로가기 기본 동작입니다. (패널이 없을 때)
        /// </summary>
        private void OnBackPressedAtRoot()
        {
            ConfirmQuit(_cts.Token).Forget();
        }

        private async UniTaskVoid InitializeDiorama(CancellationToken ct)
        {
            try
            {
                await _diorama.InitializeAsync(ct);
            }
            catch (OperationCanceledException)
            {
                // 씬이 먼저 내려간 것이므로 처리할 결과가 없음
            }
        }

        private async UniTaskVoid StartGame(CancellationToken ct)
        {
            _isStarting = true;

            try
            {
                await _diorama.DropAsync(ct);
            }
            catch (OperationCanceledException)
            {
                // 씬이 먼저 내려간 것이므로 전환하지 않음
                return;
            }

            _sceneLoader.LoadSceneAsync(_lobbySceneConfig).Forget();
        }

        /// <summary>
        /// Settings 패널을 엽니다.
        /// </summary>
        private async UniTaskVoid OpenSettings(CancellationToken ct)
        {
            // 패널이 열린 동안 디오라마를 멈춤
            _diorama.Pause();

            try
            {
                await _navigation.ShowAsync(GameConstants.UI.Keys.SETTINGS, ct);
            }
            catch (OperationCanceledException)
            {
                // 씬이 먼저 내려간 것이므로 처리할 결과가 없음
                return;
            }

            _diorama.Resume();
        }

        /// <summary>
        /// 종료 확인 후 앱을 종료합니다.
        /// </summary>
        private async UniTaskVoid ConfirmQuit(CancellationToken ct)
        {
            bool isConfirmed;

            try
            {
                isConfirmed = await _navigation.ShowAsync<MessageRequest, bool>(GameConstants.UI.Keys.MESSAGE, GameConstants.UI.Messages.QUIT, ct);
            }
            catch (OperationCanceledException)
            {
                // 씬이 먼저 내려간 것이므로 처리할 결과가 없음
                return;
            }

            if (isConfirmed == false)
            {
                return;
            }

            AppLogger.Info("[LobbyPresenter] Quit confirmed.");
            QuitApplication();
        }
        #endregion
    }
}