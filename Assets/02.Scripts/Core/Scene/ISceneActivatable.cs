namespace CodeJelly.BounceOrBreak.Core.Scene
{
    /// <summary>
    /// 씬의 활성화 시점 처리를 담당하는 인터페이스.
    /// 전환이 완전히 끝나(로딩 화면 제거 포함) 씬이 foreground에 올라온 직후 실행할 로직이 있는 씬에서 구현합니다. (게임 시작 등)
    /// </summary>
    public interface ISceneActivatable
    {
        /// <summary>
        /// 씬 전환이 완전히 끝나 씬이 foreground에 올라온 직후 호출됩니다.
        /// </summary>
        void OnActivated();
    }
}