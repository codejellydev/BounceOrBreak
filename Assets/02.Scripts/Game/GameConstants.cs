using UnityEngine;

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

        public static class Bounce
        {
            /// <summary>
            /// Bounce 방향 벡터 유효성 검증 임계값 (sqrMagnitude)
            /// </summary>
            public const float MIN_DIRECTION_MAGNITUDE_SQR = 0.0001f;

            /// <summary>
            /// 유효하지 않은 방향일 때 사용할 기본 방향 (Fallback)
            /// </summary>
            public static readonly Vector2 DEFAULT_DIRECTION = Vector2.up;
        }
    }
}