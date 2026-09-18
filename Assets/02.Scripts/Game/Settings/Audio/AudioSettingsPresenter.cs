using Cysharp.Threading.Tasks;
using R3;

namespace CodeJelly.BounceOrBreak.Game.Settings.Audio
{
    /// <summary>
    /// 오디오 설정 카테고리의 Presenter 클래스.
    /// </summary>
    public class AudioSettingsPresenter
    {
        #region Injection
        private readonly AudioSettingsService _service;

        public AudioSettingsPresenter(AudioSettingsService service)
        {
            _service = service;
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// View를 설정에 연결합니다.
        /// </summary>
        public void Bind(AudioSettingsView view, CompositeDisposable disposables)
        {
            view.UpdateDisplay(_service.Settings);

            view.MasterVolumeSlider
                .onValueChanged
                .AsObservable()
                .Subscribe(_service.SetMasterVolume)
                .AddTo(disposables);

            view.BgmVolumeSlider
                .onValueChanged
                .AsObservable()
                .Subscribe(_service.SetBgmVolume)
                .AddTo(disposables);

            view.SfxVolumeSlider
                .onValueChanged
                .AsObservable()
                .Subscribe(_service.SetSfxVolume)
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