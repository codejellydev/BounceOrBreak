using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Bootstrap;
using CodeJelly.BounceOrBreak.Game.Entry;
using CodeJelly.BounceOrBreak.Game.UI.Message;
using CodeJelly.BounceOrBreak.Game.UI.Navigation;
using CodeJelly.BounceOrBreak.Game.UI.Panel;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Scope
{
    /// <summary>
    /// EntryScene의 LifetimeScope.
    /// </summary>
    public class EntrySceneScope : LifetimeScope
    {
        #region Private Variables
        /// <summary>
        /// EntryScene에서 사용하는 Transition 설정.
        /// 이 설정 자체가 Addressables 시스템 초기화 이전에 필요하므로,
        /// ResourceManager를 통한 로드가 불가능하여 직접 참조. (EntryScene 특수성)
        /// </summary>
        [SerializeField] private SceneTransitionConfig _entrySceneConfig;

        [Header("Entry")]
        [SerializeField] private EntryView _entryView;

        [Header("Navigation")]
        [SerializeField] private UIPanelRoot _panelRoot;
        #endregion


        #region Protected Methods
        protected override void Configure(IContainerBuilder builder)
        {
            // Entry
            builder.RegisterInstance(_entrySceneConfig);
            builder.RegisterComponent(_entryView);
            builder.RegisterEntryPoint<Bootstrapper>()
                   .AsSelf();
            builder.RegisterEntryPoint<EntryPresenter>();

            // Navigation
            builder.RegisterComponent(_panelRoot);
            builder.RegisterEntryPoint<UIPanelRootRegistrar>();
            builder.Register<MessagePanelPresenter>(Lifetime.Scoped)
                   .As<IUIPanel>();
        }
        #endregion
    }
}
