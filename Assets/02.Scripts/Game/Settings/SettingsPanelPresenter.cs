using CodeJelly.BounceOrBreak.Game.Settings.Audio;
using CodeJelly.BounceOrBreak.Game.Settings.Graphics;
using CodeJelly.BounceOrBreak.Game.Settings.Language;
using CodeJelly.BounceOrBreak.Game.Settings.PlayerData;
using CodeJelly.BounceOrBreak.Game.UI.Panel;
using R3;

namespace CodeJelly.BounceOrBreak.Game.Settings
{
    /// <summary>
    /// Settings 패널의 Presenter 클래스.
    /// </summary>
    public class SettingsPanelPresenter : UIPanelPresenter<SettingsPanelView, Unit>
    {
        #region Injection
        private readonly AudioSettingsPresenter _audioPresenter;
        private readonly GraphicsSettingsPresenter _graphicsPresenter;
        private readonly LanguageSettingsPresenter _languagePresenter;
        private readonly PlayerDataPresenter _playerDataPresenter;

        public SettingsPanelPresenter(
            AudioSettingsPresenter audioPresenter,
            GraphicsSettingsPresenter graphicsPresenter,
            LanguageSettingsPresenter languagePresenter,
            PlayerDataPresenter playerDataPresenter)
        {
            _audioPresenter = audioPresenter;
            _graphicsPresenter = graphicsPresenter;
            _languagePresenter = languagePresenter;
            _playerDataPresenter = playerDataPresenter;
        }
        #endregion


        #region Properties
        public override string Key => GameConstants.UI.Keys.SETTINGS;
        public override Unit CancelResult => Unit.Default;
        #endregion


        #region Protected Methods
        protected override void Bind(SettingsPanelView view, CompositeDisposable disposables)
        {
            view.CloseButton
                .OnClick
                .Subscribe(_ => Cancel())
                .AddTo(disposables);

            view.DimBackgroundButton
                .OnClickAsObservable()
                .Subscribe(_ => Cancel())
                .AddTo(disposables);

            _audioPresenter.Bind(view.AudioView, disposables);
            _graphicsPresenter.Bind(view.GraphicsView, disposables);
            _languagePresenter.Bind(view.LanguageView, disposables);
            _playerDataPresenter.Bind(view.PlayerDataView, disposables, ShowToken);
        }

        protected override void OnClosed()
        {
            _audioPresenter.Save();
            _graphicsPresenter.Save();
            _languagePresenter.Save();
        }
        #endregion
    }
}