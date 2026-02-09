namespace CodeJelly.BounceOrBreak.Game.Reflect
{
    /// <summary>
    /// 반사 효과를 받을 수 있는 객체가 구현해야 하는 인터페이스.
    /// </summary>
    public interface IReflectable
    {
        /// <summary>
        /// 반사 벡터를 계산하여 적용합니다.
        /// </summary>
        void Reflect(ReflectParams parameters);
    }
}