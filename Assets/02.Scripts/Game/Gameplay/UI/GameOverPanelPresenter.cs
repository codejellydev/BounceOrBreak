using CodeJelly.BounceOrBreak.Core.Logging;
using R3;
using System;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.UI
{
    /// <summary>
    /// GameOver 패널의 Presenter 클래스.
    /// </summary>
    public class GameOverPanelPresenter : IStartable, IDisposable
    {
        #region Injection
        private readonly GameOverPanelView _view;
        private readonly GameFlowController _flowController;

        public GameOverPanelPresenter(
            GameOverPanelView view,
            GameFlowController flowController)
        {
            _view = view;
            _flowController = flowController;
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
            _flowController.Phase
                           .Subscribe(phase => _view.SetPanelActive(phase == GamePhase.GameOver))
                           .AddTo(_disposables);

            _flowController.OnRoundEnded
                           .Subscribe(_view.ShowResult)
                           .AddTo(_disposables);

            _view.RestartButton
                 .OnClick
                 .Subscribe(_ => OnRestartButtonClicked())
                 .AddTo(_disposables);
        }

        /// <summary>
        /// Restart 버튼 클릭 이벤트를 처리합니다.
        /// </summary>
        private void OnRestartButtonClicked()
        {
            AppLogger.Info("[GameOverPanelPresenter] Restart button clicked.");
            _flowController.RestartRound();
        }
        #endregion
    }
}
