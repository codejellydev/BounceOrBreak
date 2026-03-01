

using CodeJelly.BounceOrBreak.Core.Resource;
using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Input;
using Cysharp.Threading.Tasks;
using System.Threading;
using VContainer.Unity;

/// <summary>
/// [TEST] 두 진입 경로를 모두 처리하는 EntryPoint.
/// - EntryScene 경유: ResourceManager, InputManager가 이미 초기화된 상태 → 스킵
/// - TestScene 직접 실행: 직접 bootstrap 수행
/// 개발이 끝난 후 제거되어야 합니다.
/// </summary>
public class TestSceneBootstrapper : IAsyncStartable
{
    #region Injection
    private readonly ResourceManager _resourceManager;
    private readonly InputManager _inputManager;
    private readonly ISceneInitializer _initializer;

    public TestSceneBootstrapper(
        ResourceManager resourceManager,
        InputManager inputManager,
        ISceneInitializer initializer)
    {
        _resourceManager = resourceManager;
        _inputManager = inputManager;
        _initializer = initializer;
    }
    #endregion


    #region IAsyncStartable Implementation
    public async UniTask StartAsync(CancellationToken ct)
    {
        // 이미 초기화된 상태면 스킵 (EntryScene 경유 시)
        // 초기화 안 된 상태면 직접 수행 (TestScene 직접 실행 시)
        await _resourceManager.InitializeAsync();
        await _inputManager.InitializeAsync(ct);
        await _initializer.InitializeAsync(null, ct);
    }
    #endregion
}