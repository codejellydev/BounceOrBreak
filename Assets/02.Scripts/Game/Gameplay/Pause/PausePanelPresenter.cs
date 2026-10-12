using CodeJelly.BounceOrBreak.Game.Gameplay.Score;
using CodeJelly.BounceOrBreak.Game.Score;
using CodeJelly.BounceOrBreak.Game.UI.Panel;
using R3;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Pause
{
    /// <summary>
    /// 정지 패널의 Presenter 클래스.
    /// 나가기 선택만 Exit, 그 외 모든 닫힘은 Resume을 반환합니다.
    /// </summary>
    public class PausePanelPresenter : UIPanelPresenter<PausePanelView, PausePanelResult>
    {
        #region Injection
        private readonly RoundScoreService _roundScoreService;
        private readonly ScoreRecordService _scoreRecordService;

        public PausePanelPresenter(
            RoundScoreService roundScoreService,
            ScoreRecordService scoreRecordService)
        {
            _roundScoreService = roundScoreService;
            _scoreRecordService = scoreRecordService;
        }
        #endregion


        #region Properties
        public override string Key => GameConstants.UI.Keys.PAUSE;
        public override PausePanelResult CancelResult => PausePanelResult.Resume;
        #endregion


        #region Protected Methods
        protected override void Bind(PausePanelView view, CompositeDisposable disposables)
        {
            _roundScoreService.CurrentScore
                              .Subscribe(view.SetCurrentScore)
                              .AddTo(disposables);

            _scoreRecordService.BestScore
                               .Subscribe(view.SetBestScore)
                               .AddTo(disposables);

            view.ResumeButton
                .OnClick
                .Subscribe(_ => Complete(PausePanelResult.Resume))
                .AddTo(disposables);

            view.RestartButton
                .OnClick
                .Subscribe(_ => Complete(PausePanelResult.Restart))
                .AddTo(disposables);

            view.ExitButton
                .OnClick
                .Subscribe(_ => Complete(PausePanelResult.Exit))
                .AddTo(disposables);

            view.DimBackgroundButton
                .OnClickAsObservable()
                .Subscribe(_ => Cancel())
                .AddTo(disposables);
        }
        #endregion
    }
}