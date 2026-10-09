using CodeJelly.BounceOrBreak.Core.Scene;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Loading
{
    /// <summary>
    /// LoadingScene의 Presenter 클래스.
    /// </summary>
    public class LoadingPresenter : ITickable, ILoadingDisplay
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
        private float _displayedProgress;
        private bool _isCompleting;
        #endregion


        #region VContainer Lifecycle
        public void Tick()
        {
            float target = (_isCompleting == true) ? 1f : _sceneLoader.TransitionProgress.CurrentValue;
            float deltaTime = Mathf.Min(Time.unscaledDeltaTime, GameConstants.UI.MAX_EFFECT_DELTA_TIME);
            float next = Mathf.MoveTowards(_displayedProgress, target, GameConstants.Loading.PROGRESS_SPEED * deltaTime);

            // 실제 진행률이 낮아져도 표시는 되돌리지 않음
            if (next <= _displayedProgress)
            {
                return;
            }

            _displayedProgress = next;
            _view.UpdateProgress(_displayedProgress);
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// ILoadingDisplay.CompleteAsync Implementation
        /// </summary>
        public UniTask CompleteAsync(CancellationToken ct)
        {
            _isCompleting = true;

            return UniTask.WaitUntil(() => _displayedProgress >= 1f, cancellationToken: ct);
        }
        #endregion
    }
}