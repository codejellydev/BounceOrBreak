using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Gameplay.Score;
using R3;
using System;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.UI
{
    /// <summary>
    /// GameScene HUD의 Presenter 클래스.
    /// </summary>
    public class GameplayHudPresenter : IStartable, IDisposable
    {
        #region Injection
        private readonly GameplayHudView _view;
        private readonly GameFlowController _flowController;
        private readonly ScoreService _scoreService;
        private readonly GameSceneNavigator _navigator;

        public GameplayHudPresenter(
            GameplayHudView view,
            GameFlowController flowController,
            ScoreService scoreService,
            GameSceneNavigator navigator)
        {
            _view = view;
            _flowController = flowController;
            _scoreService = scoreService;
            _navigator = navigator;
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
            // GameOverPanel이 같은 점수를 보여주므로 HUD는 플레이 중에만 표시
            _flowController.Phase
                           .Subscribe(phase => _view.SetVisible(phase == GamePhase.Playing))
                           .AddTo(_disposables);

            _scoreService.CurrentScore
                         .Subscribe(_view.UpdateCurrentScore)
                         .AddTo(_disposables);

            _view.PauseButton
                 .OnClick
                 .Subscribe(_ => OnPauseButtonClicked())
                 .AddTo(_disposables);
        }

        /// <summary>
        /// Pause 버튼 클릭 이벤트를 처리합니다.
        /// </summary>
        private void OnPauseButtonClicked()
        {
            AppLogger.Info("[GameplayHudPresenter] Pause button clicked.");
            _navigator.RequestPause();
        }
        #endregion
    }
}
