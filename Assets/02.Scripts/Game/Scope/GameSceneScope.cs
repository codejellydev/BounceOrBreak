using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Bootstrap;
using CodeJelly.BounceOrBreak.Game.Gameplay;
using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using CodeJelly.BounceOrBreak.Game.Gameplay.Goal;
using CodeJelly.BounceOrBreak.Game.Gameplay.Pause;
using CodeJelly.BounceOrBreak.Game.Gameplay.Score;
using CodeJelly.BounceOrBreak.Game.Gameplay.UI;
using CodeJelly.BounceOrBreak.Game.Trampoline;
using CodeJelly.BounceOrBreak.Game.UI.Navigation;
using CodeJelly.BounceOrBreak.Game.UI.Panel;
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
        [SerializeField] private SceneTransitionConfig _gameSceneConfig;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private FallDetector _fallDetector;
        [SerializeField] private GoalPort[] _goalPorts;
        [SerializeField] private TrampolineDrawer _trampolineDrawer;

        [Header("UI")]
        [SerializeField] private GameplayHudView _gameplayHudView;
        [SerializeField] private GameOverPanelView _gameOverPanelView;

        [Header("Navigation")]
        [SerializeField] private UIPanelRoot _panelRoot;
        #endregion


        #region Protected Methods
        protected override void Configure(IContainerBuilder builder)
        {
            // Gameplay
            builder.RegisterInstance(_gameSceneConfig);
            builder.RegisterComponent(_mainCamera);
            builder.RegisterComponent(_fallDetector);
            builder.RegisterInstance(_goalPorts);
            builder.RegisterComponent(_trampolineDrawer);
            builder.Register<EggSpawner>(Lifetime.Scoped);
            builder.Register<RoundScoreService>(Lifetime.Scoped);
            builder.Register<RoundProgress>(Lifetime.Scoped);

            // 초기화 순서를 SceneLoader가 구동하므로 EntryPoint가 아닌 Core.Scene 인터페이스로 등록
            builder.Register<GameFlowController>(Lifetime.Scoped)
                   .As<ISceneInitializer>()
                   .As<ISceneActivatable>()
                   .AsSelf();

            builder.RegisterEntryPoint<GameSceneNavigator>()
                   .AsSelf();

            // HUD
            builder.RegisterComponent(_gameplayHudView);
            builder.RegisterEntryPoint<GameplayHudPresenter>();

            // Game Over Panel
            builder.RegisterComponent(_gameOverPanelView);
            builder.RegisterEntryPoint<GameOverPanelPresenter>();

            // Navigation
            builder.RegisterComponent(_panelRoot);
            builder.RegisterEntryPoint<UIPanelRootRegistrar>();
            builder.Register<PausePanelPresenter>(Lifetime.Scoped)
                   .As<IUIPanel>();

#if UNITY_EDITOR
            // 씬 단독 실행 시의 초기화 대행
            builder.RegisterEntryPoint<StandaloneSceneBootstrapper>();
#endif
        }
        #endregion
    }
}