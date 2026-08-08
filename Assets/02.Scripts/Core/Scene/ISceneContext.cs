namespace CodeJelly.BounceOrBreak.Core.Scene
{
    /// <summary>
    /// 씬 전환 시 Context(Payload)를 수신하는 인터페이스.
    /// </summary>
    public interface ISceneContext
    {
        /// <summary>
        /// 씬의 Context를 설정합니다.
        /// </summary>
        void SetContext(IScenePayload payload);
    }
}