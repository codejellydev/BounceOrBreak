namespace CodeJelly.BounceOrBreak.Game
{
    /// <summary>
    /// 게임 내 상수를 관리하는 컨테이너 클래스.
    /// 추후 상수가 많아질 때를 대비 partial로 선언.
    /// </summary>
    public static partial class GameConstants
    {
        public static class Scene
        {
            /// <summary>
            /// 씬 관련 Addresables 키
            /// </summary>
            public static class Keys
            {
                public const string LOADING_SCENE = "LoadingScene";
            }

        }
    }
}