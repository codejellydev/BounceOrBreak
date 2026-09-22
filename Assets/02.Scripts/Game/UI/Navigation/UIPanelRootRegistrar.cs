using CodeJelly.BounceOrBreak.Game.UI.Panel;
using System;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.UI.Navigation
{
    /// <summary>
    /// 씬 스코프의 UIPanelRoot 등록을 담당하는 클래스.
    /// </summary>
    public class UIPanelRootRegistrar : IStartable, IDisposable
    {
        #region Injection
        private readonly UINavigationService _navigation;
        private readonly UIPanelRoot _root;
        private readonly IReadOnlyList<IUIPanel> _panels;
        private readonly IObjectResolver _resolver;

        public UIPanelRootRegistrar(
            UINavigationService navigation,
            UIPanelRoot root,
            IReadOnlyList<IUIPanel> panels,
            IObjectResolver resolver)
        {
            _navigation = navigation;
            _root = root;
            _panels = panels;
            _resolver = resolver;
        }
        #endregion


        #region Private Variables
        private IDisposable _registration;
        #endregion


        #region VContainer Lifecycle
        public void Start()
        {
            _registration = _navigation.RegisterRoot(_root, _panels, _resolver);
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _registration?.Dispose();
        }
        #endregion
    }
}