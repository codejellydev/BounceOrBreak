using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Scene;
using Cysharp.Threading.Tasks;
using R3;
using System;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Lobby
{
    /// <summary>
    /// LobbyScene의 Presenter 클래스.
    /// </summary>
    public class LobbyPresenter : IStartable, IDisposable
    {
        #region Injection
        private readonly LobbyView _view;
        private readonly SceneLoader _sceneLoader;
        private readonly SceneTransitionConfig _lobbySceneConfig;

        public LobbyPresenter(
            LobbyView view, 
            SceneLoader sceneLoader, 
            SceneTransitionConfig lobbySceneConfig)
        {
            _view = view;
            _sceneLoader = sceneLoader;
            _lobbySceneConfig = lobbySceneConfig;
        }
        #endregion


        #region Private Variables
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        #endregion


        #region VContainer Lifecycle
        public void Start()
        {
            BindView();
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _disposables.Dispose();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// View와 Presenter 간의 이벤트 바인딩을 설정합니다.
        /// </summary>
        private void BindView()
        {
            _view.StartButton
                 .OnClickAsObservable()
                 .Subscribe(_ => OnStartButtonClicked())
                 .AddTo(_disposables);
        }

        /// <summary>
        /// Start 버튼 클릭 이벤트를 처리합니다.
        /// </summary>
        private void OnStartButtonClicked()
        {
            AppLogger.Info("[LobbyPresenter] Start button clicked. Transitioning to GameScene...");
            _sceneLoader.LoadSceneAsync(_lobbySceneConfig).Forget();
        }
        #endregion
    }
}