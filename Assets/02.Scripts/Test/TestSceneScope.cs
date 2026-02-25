using CodeJelly.BounceOrBreak.Core.Resource;
using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Input;
using CodeJelly.BounceOrBreak.Game.Trampoline;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// [TEST] TestScene의 LifetimeScope.
/// 개발이 끝난 후 제거되어야 합니다.
/// </summary>
public class TestSceneScope : LifetimeScope
{
    #region Private Variables
    [SerializeField] private TrampolineDrawer _trampolineDrawer;
    #endregion


    #region Protected Methods
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(_trampolineDrawer);
        builder.Register<TestSceneInitializer>(Lifetime.Scoped).AsImplementedInterfaces();
        builder.RegisterEntryPoint<TestSceneBootstrapper>();
    }
    #endregion
}