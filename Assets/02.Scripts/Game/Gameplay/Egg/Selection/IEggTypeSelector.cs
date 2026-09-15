namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// EggType 선택 방식에 대한 인터페이스.
    /// 하나의 인스턴스는 하나의 Wave에만 사용됩니다. 구현체는 Wave 간 상태 유지를 고려하지 않아도 됩니다.
    /// </summary>
    public interface IEggTypeSelector
    {
        /// <summary>
        /// 다음에 스폰할 EggType을 선택합니다.
        /// </summary>
        /// <param name="wave">EggType을 고를 Wave. Types가 하나 이상이어야 합니다.</param>
        EggType Select(WaveData wave);
    }
}
