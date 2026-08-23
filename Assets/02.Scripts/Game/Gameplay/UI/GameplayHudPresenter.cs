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
        private readonly ScoreService _scoreService;

        public GameplayHudPresenter(
            GameplayHudView view,
            ScoreService scoreService)
        {
            _view = view;
            _scoreService = scoreService;
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
            _scoreService.CurrentScore
                         .Subscribe(_view.UpdateCurrentScore)
                         .AddTo(_disposables);
        }
        #endregion
    }
}
