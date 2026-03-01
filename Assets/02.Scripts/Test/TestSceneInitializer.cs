using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Input;
using CodeJelly.BounceOrBreak.Game.Trampoline;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

/// <summary>
/// [TEST] TestScene의 초기화를 담당합니다.
/// 개발이 끝난 후 제거되어야 합니다.
/// </summary>
public class TestSceneInitializer : ISceneInitializer
{
    #region Injection
    private readonly InputManager _inputManager;
    private readonly TrampolineDrawer _trampolineDrawer;

    public TestSceneInitializer(InputManager inputManager, TrampolineDrawer trampolineDrawer)
    {
        _inputManager = inputManager;
        _trampolineDrawer = trampolineDrawer;
    }
    #endregion


    #region ISceneInitializer Implementation
    public SceneInitializationState State { get; private set; } = SceneInitializationState.None;

    public async UniTask InitializeAsync(IProgress<float> progress, CancellationToken ct)
    {
        if (State != SceneInitializationState.None)
        {
            AppLogger.Debug($"[TestSceneInitializer] State is {State}. Skipping.");
            return;
        }

        State = SceneInitializationState.Initializing;

        try
        {
            AppLogger.Debug("[TestSceneInitializer] Initialization started.");

            _inputManager.SwitchContext(InputContext.Gameplay);

            await _trampolineDrawer.InitializeAsync(
                new Progress<float>(p => progress?.Report(p)),
                ct
            );
            _trampolineDrawer.SetTrampolineType(TrampolineType.Straight);

            State = SceneInitializationState.Completed;
            AppLogger.Debug("[TestSceneInitializer] Initialization completed.");
        }
        catch (Exception ex)
        {
            State = SceneInitializationState.Failed;
            AppLogger.Exception(ex, "[TestSceneInitializer] Initialization failed.");
            throw;
        }
    }
    #endregion
}