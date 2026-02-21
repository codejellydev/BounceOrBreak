using CodeJelly.BounceOrBreak.Core.Resource;
using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Input;
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
        #region Unity Lifecycle
        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }
        #endregion


        #region Protected Methods
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<InputManager>(Lifetime.Singleton);
            builder.Register<ResourceManager>(Lifetime.Singleton);
            builder.Register<SceneLoader>(Lifetime.Singleton);
        }
        #endregion
    }
}