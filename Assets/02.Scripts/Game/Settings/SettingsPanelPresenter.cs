using CodeJelly.BounceOrBreak.Core.Logging;
using R3;
using System;
using System.Collections.Generic;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Settings
{
    /// <summary>
    /// Settings 패널의 Presenter 클래스.
    /// </summary>
    public class SettingsPanelPresenter : IStartable, IDisposable
    {
        #region Injection
        private readonly SettingsPanelView _view;
        private readonly IReadOnlyList<ISettingsCategoryPresenter> _categoryPresenters;

        public SettingsPanelPresenter(
            SettingsPanelView view,
            IReadOnlyList<ISettingsCategoryPresenter> categoryPresenters)
        {
            _view = view;
            _categoryPresenters = categoryPresenters;
        }
        #endregion


        #region Private Variables
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        #endregion


        #region VContainer Lifecycle
        public void Start()
        {
            _view.SetPanelActive(false);
            BindView();
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// Settings 패널을 열고, 등록된 모든 카테고리 Presenter에 통지합니다.
        /// </summary>
        public void Open()
        {
            _view.SetPanelActive(true);

            foreach (var presenter in _categoryPresenters)
            {
                presenter.OnPanelOpened();
            }
        }

        /// <summary>
        /// Settings 패널을 닫고, 등록된 모든 카테고리 Presenter에 통지합니다.
        /// </summary>
        public void Close()
        {
            foreach (var presenter in _categoryPresenters)
            {
                presenter.OnPanelClosed();
            }

            _view.SetPanelActive(false);
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _disposables.Dispose();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// View와 Presenter 간의 이벤트 바인딩을 설정합니다.
        /// </summary>
        private void BindView()
        {
            _view.CloseButton
                 .OnClickAsObservable()
                 .Subscribe(_ => OnCloseRequested())
                 .AddTo(_disposables);

            _view.DimBackgroundButton
                 .OnClickAsObservable()
                 .Subscribe(_ => OnCloseRequested())
                 .AddTo(_disposables);
        }

        /// <summary>
        /// 닫기 요청 이벤트를 처리합니다.
        /// </summary>
        private void OnCloseRequested()
        {
            AppLogger.Info("[SettingsPanelPresenter] Settings panel close requested.");
            Close();
        }
        #endregion
    }
}