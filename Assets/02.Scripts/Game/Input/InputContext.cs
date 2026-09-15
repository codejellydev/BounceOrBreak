namespace CodeJelly.BounceOrBreak.Game.Input
{
    /// <summary>
    /// 게임의 Context에 따른 Input Context를 정의합니다.
    /// </summary>
    public enum InputContext
    {
        None = 0,
        Pause = 1,
        Loading = 2,
        Lobby = 3,
        Gameplay = 4,
        GameOver = 5,
    }
}