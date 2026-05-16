namespace CodeJelly.BounceOrBreak.Core.Pooling
{
    /// <summary>
    /// Unity ObjectPool에서 관리되는 오브젝트가 구현해야 하는 인터페이스.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 풀에서 꺼낼 때 호출됩니다. 초기화 로직을 수행합니다.
        /// </summary>
        void Get();

        /// <summary>
        /// 풀에 반환될 때 호출됩니다. 상태 초기화 로직을 수행합니다.
        /// </summary>
        void Release();

        /// <summary>
        /// 풀에서 완전히 제거될 때 호출됩니다. 리소스 해제 로직을 수행합니다.
        /// </summary>
        void Destroy();
    }
}