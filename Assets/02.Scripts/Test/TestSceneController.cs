using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Input;
using CodeJelly.BounceOrBreak.Game.Trampoline;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// [TEST] TestScene을 관리합니다.
/// 개발이 끝난 후 제거되어야 합니다.
/// </summary>
public class TestSceneController : MonoBehaviour
{
    #region Private Variables
    [Header("Dependencies")]
    [SerializeField] private TrampolineDrawer _trampolineDrawer;
    #endregion


    #region Unity Lifecycle
    private void Start()
    {
        InitializeAsync(null, destroyCancellationToken).Forget();
    }
    #endregion


    #region ISceneInitializer Implementation
    public async UniTask InitializeAsync(IProgress<float> progress, CancellationToken ct)
    {
        AppLogger.Debug("[TestSceneController] Initialization started.");

        InputManager.Instance.SwitchContext(InputContext.Gameplay);

        // TrampolineDrawer 초기화
        await _trampolineDrawer.InitializeAsync
        (
            new Progress<float>(p => progress?.Report(p)),
            ct
        );
        _trampolineDrawer.SetTrampolineType(TrampolineType.Straight);

        AppLogger.Debug("[GameplaySceneBootstrap] Initialization completed.");
    }
    #endregion
}