using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Gameplay;
using VContainer;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Scope
{
    /// <summary>
    /// GameScene의 LifetimeScope.
    /// </summary>
    public class GameSceneScope : LifetimeScope
    {
        #region Protected Methods
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameFlowController>(Lifetime.Scoped)
                   .As<ISceneInitializer>()
                   .As<ISceneActivatable>() // VContainer EntryPoint가 아닌, Core.Scene의 ISceneActivator로 등록 (씬 활성화 시점에 자동으로 OnActivated 호출)
                   .AsSelf();
        }
        #endregion
    }
}