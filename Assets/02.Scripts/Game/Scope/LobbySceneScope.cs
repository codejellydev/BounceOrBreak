using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Bootstrap;
using CodeJelly.BounceOrBreak.Game.Lobby;
using CodeJelly.BounceOrBreak.Game.Settings;
using CodeJelly.BounceOrBreak.Game.Settings.Audio;
using CodeJelly.BounceOrBreak.Game.Settings.Graphics;
using CodeJelly.BounceOrBreak.Game.Settings.Language;
using CodeJelly.BounceOrBreak.Game.UI.Navigation;
using CodeJelly.BounceOrBreak.Game.UI.Panel;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Scope
{
    /// <summary>
    /// LobbyScene의 LifetimeScope.
    /// </summary>
    public class LobbySceneScope : LifetimeScope
    {
        #region Private Variables
        [Header("Lobby")]
        [SerializeField] private SceneTransitionConfig _lobbySceneConfig;
        [SerializeField] private LobbyView _lobbyView;

        [Header("Navigation")]
        [SerializeField] private UIPanelRoot _panelRoot;
        #endregion


        #region Protected Methods
        protected override void Configure(IContainerBuilder builder)
        {
            // Lobby
            builder.RegisterInstance(_lobbySceneConfig);
            builder.RegisterComponent(_lobbyView);
            builder.RegisterEntryPoint<LobbyPresenter>()
                   .As<ISceneActivatable>();

            // Navigation
            builder.RegisterComponent(_panelRoot);
            builder.RegisterEntryPoint<UIPanelRootRegistrar>();
            builder.Register<QuitConfirmPresenter>(Lifetime.Scoped)
                   .As<IUIPanel>();
            builder.Register<SettingsPanelPresenter>(Lifetime.Scoped)
                   .As<IUIPanel>();

            // Settings Panel
            builder.Register<AudioSettingsPresenter>(Lifetime.Scoped);
            builder.Register<GraphicsSettingsPresenter>(Lifetime.Scoped);
            builder.Register<LanguageSettingsPresenter>(Lifetime.Scoped);

#if UNITY_EDITOR
            // 씬 단독 실행 시의 초기화 대행
            builder.RegisterEntryPoint<StandaloneSceneBootstrapper>();
#endif
        }
        #endregion
    }
}
