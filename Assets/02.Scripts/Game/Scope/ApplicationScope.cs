using CodeJelly.BounceOrBreak.Core.Localization;
using CodeJelly.BounceOrBreak.Core.Resource;
using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Core.Settings;
using CodeJelly.BounceOrBreak.Core.UI;
using CodeJelly.BounceOrBreak.Game.Bootstrap;
using CodeJelly.BounceOrBreak.Game.Input;
using CodeJelly.BounceOrBreak.Game.Score;
using CodeJelly.BounceOrBreak.Game.Settings.Audio;
using CodeJelly.BounceOrBreak.Game.Settings.Graphics;
using CodeJelly.BounceOrBreak.Game.Settings.Language;
using CodeJelly.BounceOrBreak.Game.UI.Navigation;
using CodeJelly.BounceOrBreak.Game.UI.Overlay;
using CodeJelly.BounceOrBreak.Game.UI.Panel;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Scope
{
    /// <summary>
    /// 애플리케이션 수준의 LifetimeScope로, 모든 LifetimeScope의 최상위에 위치합니다.
    /// 씬 전환과 무관하게 유지되어야 하는 시스템을 등록하고 관리합니다.
    /// </summary>
    public class ApplicationScope : LifetimeScope
    {
        #region Private Variables
        [Header("Audio Settings")]
        [SerializeField] private AudioMixerConfig _audioMixerConfig;

        [Header("Graphics Settings")]
        [SerializeField] private GraphicsPresetConfig _graphicsPresetConfig;

        [Header("UI")]
        [SerializeField] private UIPanelConfig _uiPanelConfig;

        [Tooltip("씬 전환과 무관하게 유지되는(앱 수준) 오버레이가 생성될 자리")]
        [SerializeField] private Transform _overlayRoot;

        [Tooltip("전환 연출 프리팹. ITransitionEffect로 등록됨")]
        [SerializeField] private ScreenFader _screenFaderPrefab;
        #endregion


        #region Protected Methods
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<InputManager>(Lifetime.Singleton);
            builder.Register<ResourceManager>(Lifetime.Singleton);
            builder.Register<LocalizationManager>(Lifetime.Singleton);
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register<AppInitializer>(Lifetime.Singleton);

            builder.Register<ISettingsRepository, PlayerPrefsSettingsRepository>(Lifetime.Singleton);
            builder.RegisterInstance(_audioMixerConfig);
            builder.RegisterInstance(_graphicsPresetConfig);
            builder.Register<IAudioSettingsApplier, AudioMixerApplier>(Lifetime.Singleton);
            builder.Register<IGraphicsSettingsApplier, UnityGraphicsApplier>(Lifetime.Singleton);
            builder.Register<ILanguageSettingsApplier, LocalizationApplier>(Lifetime.Singleton);
            builder.Register<AudioSettingsService>(Lifetime.Singleton);
            builder.Register<GraphicsSettingsService>(Lifetime.Singleton);
            builder.Register<LanguageSettingsService>(Lifetime.Singleton);
            builder.Register<ScoreRecordService>(Lifetime.Singleton);

            builder.RegisterComponentInNewPrefab(_screenFaderPrefab, Lifetime.Singleton)
                   .UnderTransform(_overlayRoot)
                   .As<ITransitionEffect>();

            builder.RegisterInstance(_uiPanelConfig);
            builder.RegisterEntryPoint<UINavigationService>()
                   .AsSelf();
        }
        #endregion
    }
}