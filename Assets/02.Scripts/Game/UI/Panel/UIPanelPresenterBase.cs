using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.UI.Panel
{
    /// <summary>
    /// 패널 Presenter의 공통 바탕 클래스.
    /// </summary>
    public abstract class UIPanelPresenterBase<TView, TResult> where TView : Component
    {
        #region Properties
        public abstract string Key { get; }
        public abstract TResult CancelResult { get; }

        /// <summary>
        /// 패널이 열려 있는 동안 유효한 토큰
        /// </summary>
        protected CancellationToken ShowToken { get; private set; }
        #endregion


        #region Private Variables
        private readonly CompositeDisposable _bindings = new CompositeDisposable();

        private UniTaskCompletionSource<TResult> _completion;
        #endregion


        #region Public Methods
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
        /// 인스턴스에 바인딩해 닫힐 때까지 기다립니다.
        /// </summary>
        protected async UniTask<TResult> RunAsync(GameObject instance, CancellationToken ct, Action<TView, CompositeDisposable> bind)
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
                ShowToken = ct;
                bind(view, _bindings);

                try
                {
                    return await completion.Task;
                }
                finally
                {
                    _bindings.Clear();
                    _completion = null;
                    ShowToken = CancellationToken.None;

                    OnClosed();
                }
            }
        }

        /// <summary>
        /// 패널이 닫힌 뒤 호출됩니다.
        /// </summary>
        protected virtual void OnClosed()
        { }

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