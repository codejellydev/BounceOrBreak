using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.UI.Panel
{
    /// <summary>
    /// IUIPanel<TResult>의 공통 구현체.
    /// 구현체는 Bind만 채웁니다.
    /// </summary>
    public abstract class UIPanelPresenter<TView, TResult> : IUIPanel<TResult> where TView : Component
    {
        #region Properties
        public abstract string Key { get; }
        public abstract TResult CancelResult { get; }
        #endregion


        #region Private Variables
        private readonly CompositeDisposable _bindings = new CompositeDisposable();

        private UniTaskCompletionSource<TResult> _completion;
        #endregion


        #region Public Methods
        /// <summary>
        /// IUIPanel.RunAsync Implementation
        /// </summary>
        public async UniTask<TResult> RunAsync(GameObject instance, CancellationToken ct)
        {
            if (_completion != null)
            {
                throw new InvalidOperationException($"[{GetType().Name}] {Key} is already open.");
            }

            if (instance.TryGetComponent(out TView view) == false)
            {
                throw new InvalidOperationException($"[{GetType().Name}] {instance.name} has no {typeof(TView).Name}.");
            }

            UniTaskCompletionSource<TResult> completion = new UniTaskCompletionSource<TResult>();
            _completion = completion;

            // 씬이 끝나면 호출자의 await도 풀려야 하므로 취소를 완료에 연결
            using (ct.Register(() => completion.TrySetCanceled(ct)))
            {
                Bind(view, _bindings);

                try
                {
                    return await completion.Task;
                }
                finally
                {
                    _bindings.Clear();
                    _completion = null;
                }
            }
        }

        /// <summary>
        /// IUIPanel.Cancel Implementation
        /// </summary>
        public void Cancel()
        {
            Complete(CancelResult);
        }
        #endregion


        #region Protected Methods
        /// <summary>
        /// View 이벤트를 결과에 연결합니다. disposables에 담은 구독은 닫힐 때 해제됩니다.
        /// </summary>
        protected abstract void Bind(TView view, CompositeDisposable disposables);

        /// <summary>
        /// 결과를 확정하고 패널을 닫습니다. 이미 닫혔으면 무시합니다.
        /// </summary>
        protected void Complete(TResult result)
        {
            _completion?.TrySetResult(result);
        }
        #endregion
    }
}