namespace CodeJelly.BounceOrBreak.Game.Bounce
{
    /// <summary>
    /// 튕김 효과를 받을 수 있는 객체가 구현해야 하는 인터페이스.
    /// </summary>
    public interface IBounceable
    {
        /// <summary>
        /// 튕김 파라미터를 받아 물리적 튕김을 적용합니다.
        /// </summary>
        void Bounce(BounceParams parameters);

        /// <summary>
        /// 튕김 결과를 받아 추가 효과를 처리합니다. (선택적 구현 가능)
        /// 이펙트, 사운드 등 피드백 처리에 사용합니다.
        /// </summary>
        void OnBounced(BounceResult result);
    }
}