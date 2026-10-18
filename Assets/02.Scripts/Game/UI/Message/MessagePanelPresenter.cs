using CodeJelly.BounceOrBreak.Game.UI.Panel;
using R3;

namespace CodeJelly.BounceOrBreak.Game.UI.Message
{
    /// <summary>
    /// 범용 메시지 패널의 Presenter 클래스.
    /// 주 버튼이면 true, 그 외 모든 닫힘은 false를 반환합니다.
    /// </summary>
    public class MessagePanelPresenter : UIPanelPresenter<MessagePanelView, MessageRequest, bool>
    {
        #region Properties
        public override string Key => GameConstants.UI.Keys.MESSAGE;
        public override bool CancelResult => false;
        #endregion


        #region Protected Methods
        protected override void Bind(MessagePanelView view, MessageRequest request, CompositeDisposable disposables)
        {
            view.Show(request);

            view.PrimaryButton
                .OnClick
                .Subscribe(_ => Complete(true))
                .AddTo(disposables);

            view.SecondaryButton
                .OnClick
                .Subscribe(_ => Cancel())
                .AddTo(disposables);

            view.DimBackgroundButton
                .OnClickAsObservable()
                .Subscribe(_ => Cancel())
                .AddTo(disposables);
        }
        #endregion
    }
}