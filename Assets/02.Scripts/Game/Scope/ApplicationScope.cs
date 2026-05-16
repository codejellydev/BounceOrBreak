using CodeJelly.BounceOrBreak.Core.Localization;
using CodeJelly.BounceOrBreak.Core.Resource;
using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Core.Settings;
using CodeJelly.BounceOrBreak.Game.Input;
using CodeJelly.BounceOrBreak.Game.Settings.Audio;
using CodeJelly.BounceOrBreak.Game.Settings.Graphics;
using CodeJelly.BounceOrBreak.Game.Settings.Language;
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
        [SerializeField] private AudioMixerConfig _audioSettingsConfig;
        #endregion


        #region Protected Methods
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<InputManager>(Lifetime.Singleton);
            builder.Register<ResourceManager>(Lifetime.Singleton);
            builder.Register<LocalizationManager>(Lifetime.Singleton);
            builder.Register<SceneLoader>(Lifetime.Singleton);

            builder.Register<ISettingsRepository, PlayerPrefsSettingsRepository>(Lifetime.Singleton);
            builder.RegisterInstance(_audioSettingsConfig);
            builder.Register<IAudioSettingsApplier, AudioMixerApplier>(Lifetime.Singleton);
            builder.Register<IGraphicsSettingsApplier, UnityGraphicsApplier>(Lifetime.Singleton);
            builder.Register<ILanguageSettingsApplier, LocalizationApplier>(Lifetime.Singleton);
        }
        #endregion
    }
}