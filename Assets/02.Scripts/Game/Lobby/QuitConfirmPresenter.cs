using CodeJelly.BounceOrBreak.Game.UI.Panel;
using R3;

namespace CodeJelly.BounceOrBreak.Game.Lobby
{
    /// <summary>
    /// 앱 종료 확인 패널의 Presenter 클래스.
    /// 확인이면 true, 그 외 모든 닫힘은 false를 반환합니다.
    /// </summary>
    public class QuitConfirmPresenter : UIPanelPresenter<QuitConfirmView, bool>
    {
        #region Properties
        public override string Key => GameConstants.UI.Keys.QUIT_CONFIRM;
        public override bool CancelResult => false;
        #endregion


        #region Protected Methods
        protected override void Bind(QuitConfirmView view, CompositeDisposable disposables)
        {
            view.ConfirmButton
                .OnClick
                .Subscribe(_ => Complete(true))
                .AddTo(disposables);

            view.CancelButton
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