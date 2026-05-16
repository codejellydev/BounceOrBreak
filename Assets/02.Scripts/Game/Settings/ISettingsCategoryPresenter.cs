namespace CodeJelly.BounceOrBreak.Game.Settings
{
    /// <summary>
    /// Settings 패널 내 개별 카테고리(ex. Audio, Graphics) Presenter의 인터페이스.
    /// </summary>
    public interface ISettingsCategoryPresenter
    {
        /// <summary>
        /// Settings 패널이 열릴 때 호출됩니다.
        /// 현재 설정값을 View에 동기화하는 등의 작업을 수행합니다.
        /// </summary>
        void OnPanelOpened();

        /// <summary>
        /// Settings 패널이 닫힐 때 호출됩니다.
        /// 변경사항 저장, 정리 작업 등을 수행합니다.
        /// </summary>
        void OnPanelClosed();
    }
}