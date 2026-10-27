using Cysharp.Threading.Tasks;

namespace CodeJelly.BounceOrBreak.Core.Resource
{
    /// <summary>
    /// 리소스 로더 인터페이스.
    /// </summary>
    public interface IResourceLoader
    {
        /// <summary>
        /// 리소스 시스템을 초기화합니다.
        /// </summary>
        UniTask<bool> InitializeAsync();

        /// <summary>
        /// 리소스 로드를 시작하고 핸들을 반환합니다.
        /// </summary>
        IResourceHandle Load<T>(string key) where T : UnityEngine.Object;
    }
}