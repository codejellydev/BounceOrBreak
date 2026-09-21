using CodeJelly.BounceOrBreak.Core.Scene;
using R3;
using System;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Loading
{
    /// <summary>
    /// LoadingScene의 Presenter 클래스.
    /// </summary>
    public class LoadingPresenter : IStartable, IDisposable
    {
        #region Injection
        private readonly LoadingView _view;
        private readonly SceneLoader _sceneLoader;

        public LoadingPresenter(LoadingView view, SceneLoader sceneLoader)
        {
            _view = view;
            _sceneLoader = sceneLoader;
        }
        #endregion


        #region Private Variables
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        #endregion


        #region VContainer Lifecycle
        public void Start()
        {
            _sceneLoader.TransitionProgress
                        .Subscribe(_view.UpdateProgress)
                        .AddTo(_disposables);
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
    }
}