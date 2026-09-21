using Cysharp.Threading.Tasks;
using R3;

namespace CodeJelly.BounceOrBreak.Game.Settings.Graphics
{
    /// <summary>
    /// 그래픽 설정 카테고리의 Presenter 클래스.
    /// </summary>
    public class GraphicsSettingsPresenter
    {
        #region Injection
        private readonly GraphicsSettingsService _service;

        public GraphicsSettingsPresenter(GraphicsSettingsService service)
        {
            _service = service;
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// View를 설정에 연결합니다.
        /// </summary>
        public void Bind(GraphicsSettingsView view, CompositeDisposable disposables)
        {
            view.UpdateDisplay(_service.Settings);

            view.LowQualityToggle
                .onValueChanged
                .AsObservable()
                .Where(isOn => isOn)
                .Subscribe(_ => _service.SetQualityLevel(GameConstants.Graphics.Quality.LOW))
                .AddTo(disposables);

            view.MediumQualityToggle
                .onValueChanged
                .AsObservable()
                .Where(isOn => isOn)
                .Subscribe(_ => _service.SetQualityLevel(GameConstants.Graphics.Quality.MEDIUM))
                .AddTo(disposables);

            view.HighQualityToggle
                .onValueChanged
                .AsObservable()
                .Where(isOn => isOn)
                .Subscribe(_ => _service.SetQualityLevel(GameConstants.Graphics.Quality.HIGH))
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