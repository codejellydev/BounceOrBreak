namespace CodeJelly.BounceOrBreak.Core.Scene
{
    /// <summary>
    /// Scene 전환 시 컨텍스트(Payload)를 수신하는 인터페이스.
    /// </summary>
    public interface ISceneContext
    {
        /// <summary>
        /// Scene의 컨텍스트를 설정합니다.
        /// </summary>
        void SetContext(IPayload payload);
    }
}