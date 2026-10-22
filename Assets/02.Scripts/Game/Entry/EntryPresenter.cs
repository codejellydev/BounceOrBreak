using CodeJelly.BounceOrBreak.Core;
using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Bootstrap;
using CodeJelly.BounceOrBreak.Game.UI.Message;
using CodeJelly.BounceOrBreak.Game.UI.Navigation;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Entry
{
    /// <summary>
    /// EntryScene의 Presenter 클래스.
    /// </summary>
    public class EntryPresenter : IStartable, IDisposable
    {
        #region Injection
        private readonly EntryView _view;
        private readonly Bootstrapper _bootstrapper;
        private readonly UINavigationService _navigation;

        public EntryPresenter(
            EntryView view,
            Bootstrapper bootstrapper,
            UINavigationService navigation)
        {
            _view = view;
            _bootstrapper = bootstrapper;
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
            _view.SetVersion(string.Format(GameConstants.Entry.VERSION_FORMAT, Application.version));

            _bootstrapper.StartupFailed
                         .Subscribe(_ => ShowStartupFailure(_cts.Token).Forget())
                         .AddTo(_disposables);
        }
        #endregion


        #region Public Methods
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
        private async UniTaskVoid ShowStartupFailure(CancellationToken ct)
        {
            try
            {
                await _navigation.ShowAsync<MessageRequest, bool>(GameConstants.UI.Keys.MESSAGE, GameConstants.UI.Messages.STARTUP_FAILED, ct);
            }
            catch (OperationCanceledException)
            {
                // 씬이 먼저 내려간 것이므로 처리할 결과가 없음
                return;
            }
            catch (Exception ex)
            {
                // 안내를 못 띄워도 멈춘 화면으로 두지 않도록 종료
                AppLogger.Exception(ex, "[EntryPresenter] Failed to show the startup failure message.");
            }

            ApplicationUtility.Quit();
        }
        #endregion
    }
}