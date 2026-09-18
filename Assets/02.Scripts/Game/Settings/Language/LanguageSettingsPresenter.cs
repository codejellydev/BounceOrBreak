using Cysharp.Threading.Tasks;
using R3;

namespace CodeJelly.BounceOrBreak.Game.Settings.Language
{
    /// <summary>
    /// 언어 설정 카테고리의 Presenter 클래스.
    /// </summary>
    public class LanguageSettingsPresenter
    {
        #region Injection
        private readonly LanguageSettingsService _service;

        public LanguageSettingsPresenter(LanguageSettingsService service)
        {
            _service = service;
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// View를 설정에 연결합니다.
        /// </summary>
        public void Bind(LanguageSettingsView view, CompositeDisposable disposables)
        {
            view.UpdateDisplay(_service.Settings);

            view.EnglishToggle
                .onValueChanged
                .AsObservable()
                .Where(isOn => isOn)
                .Subscribe(_ => _service.SetLocale(GameConstants.Language.LocaleCode.ENGLISH))
                .AddTo(disposables);
        }

        /// <summary>
        /// 현재 설정을 저장합니다.
        /// </summary>
        public void Save()
        {
            _service.SaveAsync().Forget();
        }
        #endregion
    }
}