using Cysharp.Threading.Tasks;
using R3;
using System.Threading;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.UI.Panel
{
    /// <summary>
    /// IUIPanel<TResult>의 공통 구현체.
    /// 구현체는 Bind만 채웁니다.
    /// </summary>
    public abstract class UIPanelPresenter<TView, TResult> : UIPanelPresenterBase<TView, TResult>, IUIPanel<TResult> where TView : Component
    {
        #region Public Methods
        /// <summary>
        /// IUIPanel.RunAsync Implementation
        /// </summary>
        public UniTask<TResult> RunAsync(GameObject instance, CancellationToken ct)
        {
            return RunAsync(instance, ct, Bind);
        }
        #endregion


        #region Protected Methods
        /// <summary>
        /// View 이벤트를 결과에 연결합니다. disposables에 담은 구독은 닫힐 때 해제됩니다.
        /// </summary>
        protected abstract void Bind(TView view, CompositeDisposable disposables);
        #endregion
    }

    /// <summary>
    /// IUIPanel<TRequest, TResult>의 공통 구현체.
    /// 구현체는 Bind만 채웁니다.
    /// </summary>
    public abstract class UIPanelPresenter<TView, TRequest, TResult> : UIPanelPresenterBase<TView, TResult>, IUIPanel<TRequest, TResult> where TView : Component
    {
        #region Public Methods
        /// <summary>
        /// IUIPanel.RunAsync Implementation
        /// </summary>
        public UniTask<TResult> RunAsync(GameObject instance, TRequest request, CancellationToken ct)
        {
            return RunAsync(instance, ct, (view, disposables) => Bind(view, request, disposables));
        }
        #endregion


        #region Protected Methods
        /// <summary>
        /// View 이벤트를 결과에 연결합니다. disposables에 담은 구독은 닫힐 때 해제됩니다.
        /// </summary>
        protected abstract void Bind(TView view, TRequest request, CompositeDisposable disposables);
        #endregion
    }
}