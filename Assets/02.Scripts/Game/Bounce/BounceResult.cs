namespace CodeJelly.BounceOrBreak.Game.Bounce
{
    /// <summary>
    /// 튕김 출력 데이터.
    /// TODO: MemoryPack 직렬화 고려 (성능 최적화 시)
    /// </summary>
    public struct BounceResult
    {
        /// <summary>
        /// 튕김 에너지.
        /// </summary>
        public float Energy;

        /// <summary>
        /// 중앙 착지 여부.
        /// </summary>
        public bool IsCenter;
    }
}