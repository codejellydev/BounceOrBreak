using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Lobby;
using CodeJelly.BounceOrBreak.Game.Settings;
using CodeJelly.BounceOrBreak.Game.Settings.Audio;
using CodeJelly.BounceOrBreak.Game.Settings.Graphics;
using CodeJelly.BounceOrBreak.Game.Settings.Language;
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
        /// <summary>
        /// LobbyScene에서 사용하는 Transition 설정.
        /// </summary>
        [SerializeField] private SceneTransitionConfig _lobbySceneConfig;
        [SerializeField] private LobbyView _lobbyView;

        [Header("Settings")]
        [SerializeField] private SettingsPanelView _settingsPanelView;
        [SerializeField] private AudioSettingsView _audioSettingsView;
        [SerializeField] private GraphicsSettingsView _graphicsSettingsView;
        [SerializeField] private LanguageSettingsView _languageSettingsView;
        #endregion


        #region Protected Methods
        protected override void Configure(IContainerBuilder builder)
        {
            // Lobby
            builder.RegisterInstance(_lobbySceneConfig);
            builder.RegisterComponent(_lobbyView);
            builder.RegisterEntryPoint<LobbyPresenter>(); // 구체 타입이지만

            // Settings Panel
            builder.RegisterComponent(_settingsPanelView);
            builder.RegisterEntryPoint<SettingsPanelPresenter>()
                   .AsSelf(); // TODO: ISettingsPanelPresenter 인터페이스로 추후 변경 예정 (구체 타입 노출 최소화)

            // Audio Settings
            builder.RegisterComponent(_audioSettingsView);
            builder.RegisterEntryPoint<AudioSettingsPresenter>()
                   .As<ISettingsCategoryPresenter>();

            // Graphics Settings
            builder.RegisterComponent(_graphicsSettingsView);
            builder.RegisterEntryPoint<GraphicsSettingsPresenter>()
                   .As<ISettingsCategoryPresenter>();

            // Language Settings
            builder.RegisterComponent(_languageSettingsView);
            builder.RegisterEntryPoint<LanguageSettingsPresenter>()
                   .As<ISettingsCategoryPresenter>();
        }
        #endregion
    }
}
