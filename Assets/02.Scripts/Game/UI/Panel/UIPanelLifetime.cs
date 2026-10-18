namespace CodeJelly.BounceOrBreak.Game.UI.Panel
{
    /// <summary>
    /// UI 패널 인스턴스의 수명 정책을 정의합니다.
    /// </summary>
    public enum UIPanelLifetime
    {
        /// <summary>
        /// 씬 진입 시 로드하고 씬이 끝날 때까지 유지
        /// </summary>
        Persistent = 0,

        /// <summary>
        /// 최초 요청 시 로드하고 이후 유지
        /// </summary>
        Cached = 1,

        /// <summary>
        /// 최초 요청 시 로드하고 닫으면 파괴
        /// </summary>
        Transient = 2,

        /// <summary>
        /// 씬의 UIPanelRoot에 미리 놓여 있어 로드하지 않음
        /// </summary>
        Resident = 3,
    }
}