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
                public const string ENTRY_SCENE = "EntryScene";
                public const string LOADING_SCENE = "LoadingScene";
            }

        }

        public static class Bounce
        {
            /// <summary>
            /// 유효하지 않은 방향일 때 사용할 기본 방향 (Fallback)
            /// </summary>
            public static readonly Vector2 DEFAULT_DIRECTION = Vector2.up;
        }

        public static class Reflect
        {
            /// <summary>
            /// 기본 속도 감쇠 비율
            /// </summary>
            public const float DEFAULT_DAMPING = 0.6f;
        }

        public static class Trampoline
        {
            /// <summary>
            /// Addressables 키
            /// </summary>
            public static class Keys
            {
                public const string STRAIGHT_PREFAB = "StraightTrampolinePrefab";
                public const string FREEFORM_PREFAB = "FreeformTrampolinePrefab";
                public const string CONFIG = "TrampolineConfig";
            }

            /// <summary>
            /// 드로잉 중 포인트 간 최소 거리 (너무 밀집된 포인트 방지)
            /// </summary>
            public const float MIN_POINT_DISTANCE = 0.1f;

            /// <summary>
            /// 라인에 저장할 수 있는 최대 포인트 개수
            /// </summary>
            public const int MAX_LINE_POINTS = 100;

            /// <summary>
            /// Trampoline 생성에 필요한 최소 포인트 개수
            /// </summary>
            public const int MIN_DRAW_POINTS = 2;

            /// <summary>
            /// 기본 라인 너비
            /// </summary>
            public const float DEFAULT_LINE_WIDTH = 0.1f;

            /// <summary>
            /// 기본 튕김 세기
            /// </summary>
            public const float DEFAULT_BOUNCE_FORCE = 10f;

            /// <summary>
            /// 기본 중앙 착지 시 튕김 배율
            /// </summary>
            public const float DEFAULT_CENTER_BOUNCE_MULTIPLIER = 1.5f;

            /// <summary>
            /// 기본 중앙 판정 범위
            /// </summary>
            public const float DEFAULT_CENTER_THRESHOLD = 0.2f;

            /// <summary>
            /// ITrampoline 오브젝트 풀의 초기 용량
            /// </summary>
            public const int POOL_DEFAULT_CAPACITY = 20;

            /// <summary>
            /// ITrampoline 오브젝트 풀의 최대 크기
            /// </summary>
            public const int POOL_MAX_SIZE = 50;
        }

        public static class Character
        {
            /// <summary>
            /// 캐릭터가 받을 수 있는 최소 튕김 힘
            /// </summary>
            public const float MIN_BOUNCE_FORCE = 5f;

            /// <summary>
            /// 캐릭터가 받을 수 있는 최대 튕김 힘
            /// </summary>
            public const float MAX_BOUNCE_FORCE = 30f;

            /// <summary>
            /// 속도가 튕김 힘에 미치는 영향 계수
            /// </summary>
            public const float SPEED_BOUNCE_MULTIPLIER = 0.05f;
        }
    }
}