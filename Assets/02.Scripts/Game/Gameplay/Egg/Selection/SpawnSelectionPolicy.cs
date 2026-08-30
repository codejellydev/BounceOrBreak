namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// Wave에서 다음 EggType을 선택하는 방식을 정의합니다.
    /// </summary>
    public enum SpawnSelectionPolicy
    {
        /// <summary>
        /// 한 주기 안에서 정의된 비율이 지켜지도록 무작위 순서로 선택
        /// </summary>
        Balanced = 0,

        /// <summary>
        /// 매번 독립적으로 무작위 선택 (같은 종류가 연속될 수 있음)
        /// </summary>
        Random = 1,
    }
}
