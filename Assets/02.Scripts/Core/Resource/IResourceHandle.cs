using Cysharp.Threading.Tasks;

namespace CodeJelly.BounceOrBreak.Core.Resource
{
    /// <summary>
    /// 리소스 핸들 인터페이스. 로드 완료 대기와 해제를 맡습니다.
    /// </summary>
    public interface IResourceHandle
    {
        /// <summary>
        /// 로드 완료를 기다리는 작업. 실패하면 null.
        /// </summary>
        UniTask<UnityEngine.Object> Task { get; }

        /// <summary>
        /// 리소스를 해제합니다.
        /// </summary>
        void Release();
    }
}