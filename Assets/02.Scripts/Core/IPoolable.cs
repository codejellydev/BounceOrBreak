namespace CodeJelly.BounceOrBreak.Core
{
    /// <summary>
    /// Unity ObjectPool에서 관리되는 오브젝트가 구현해야 하는 인터페이스.
    /// </summary>
    public interface IPoolable
    {
        void Get();
        void Release();
        void Destroy();
    }
}