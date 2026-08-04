using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Bootstrap;
using CodeJelly.BounceOrBreak.Game.Gameplay;
using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using CodeJelly.BounceOrBreak.Game.Gameplay.Goal;
using CodeJelly.BounceOrBreak.Game.Trampoline;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Scope
{
    /// <summary>
    /// GameScene의 LifetimeScope.
    /// </summary>
    public class GameSceneScope : LifetimeScope
    {
        #region Private Variables
        [Header("Gameplay")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private FallDetector _fallDetector;
        [SerializeField] private GoalPort[] _goalPorts;
        [SerializeField] private TrampolineDrawer _trampolineDrawer;
        #endregion


        #region Protected Methods
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_mainCamera);
            builder.RegisterComponent(_fallDetector);
            builder.RegisterInstance(_goalPorts);
            builder.RegisterComponent(_trampolineDrawer);

            builder.Register<EggSpawner>(Lifetime.Scoped);
            builder.Register<GameFlowController>(Lifetime.Scoped)
                   .As<ISceneInitializer>()
                   .As<ISceneActivatable>() // VContainer EntryPoint가 아닌, Core.Scene의 ISceneActivatable로 등록 (씬 활성화 시점에 자동으로 OnActivated 호출)
                   .AsSelf();

#if UNITY_EDITOR
            // Scene 단독 실행 시의 초기화 대행
            builder.RegisterEntryPoint<StandaloneSceneBootstrapper>();
#endif
        }
        #endregion
    }
}