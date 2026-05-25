using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace CodeJelly.BounceOrBreak.Core.Scene
{
    /// <summary>
    /// Scene의 초기화를 담당하는 인터페이스.
    /// 순서가 중요한 초기화가 필요한 Scene에서 구현합니다. (분할 초기화, 진행률 보고 등 지원)
    /// </summary>
    public interface ISceneInitializer
    {
        /// <summary>
        /// Scene 초기화 상태
        /// </summary>
        SceneInitializationState InitializationState { get; }

        /// <summary>
        /// Scene을 비동기적으로 초기화합니다.
        /// Scene 로드 직후 필요한 초기화 작업을 순차적으로 수행합니다.
        /// </summary>
        UniTask InitializeAsync(IProgress<float> progress, CancellationToken ct);
    }
}