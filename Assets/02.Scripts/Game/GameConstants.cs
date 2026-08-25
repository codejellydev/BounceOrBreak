using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game
{
    /// <summary>
    /// 게임 내 상수를 관리하는 컨테이너 클래스.
    /// 추후 상수가 많아질 때를 대비 partial로 선언.
    /// </summary>
    public static partial class GameConstants
    {
        public static class Input
        {
            public static class Keys
            {
                public const string DEFAULT_CONFIG = "DefaultInputConfig";
            }
        }

        public static class Scene
        {
            /// <summary>
            /// 씬 관련 Addressables 키
            /// </summary>
            public static class Keys
            {
                public const string ENTRY_SCENE = "EntryScene";
                public const string LOADING_SCENE = "LoadingScene";
                public const string LOBBY_SCENE = "LobbyScene";
                public const string GAME_SCENE = "GameScene";
            }

        }

        public static class Stage
        {
            /// <summary>
            /// Addressables 키
            /// </summary>
            public static class Keys
            {
                public const string DEFAULT_CONFIG = "DefaultStageConfig";
            }
        }

        public static class Egg
        {
            /// <summary>
            /// Addressables 키
            /// </summary>
            public static class Keys
            {
                public const string PREFAB = "EggPrefab";
                public const string REGISTRY = "EggRegistry";
                public const string SPAWN_CONFIG = "EggSpawnConfig";
            }

            /// <summary>
            /// Egg 오브젝트 Pool의 초기 용량
            /// </summary>
            public const int POOL_DEFAULT_CAPACITY = 20;

            /// <summary>
            /// Egg 오브젝트 Pool의 최대 크기
            /// </summary>
            public const int POOL_MAX_SIZE = 50;

            /// <summary>
            /// 생성된 Egg들이 배치될 루트 GameObject 이름
            /// </summary>
            public const string ROOT_NAME = "EggRoot";
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
                public const string REGISTRY = "TrampolineRegistry";
                public const string DEFAULT_CONFIG = "DefaultTrampolineConfig";
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
            /// 기본 노말 방향 가중치 (0 = 완전 반사, 1 = 완전 노말)
            /// </summary>
            public const float DEFAULT_NORMAL_BIAS = 0.5f;

            /// <summary>
            /// 기본 중앙 착지 시 튕김 배율
            /// </summary>
            public const float DEFAULT_CENTER_BOUNCE_MULTIPLIER = 1.5f;

            /// <summary>
            /// 기본 중앙 판정 범위
            /// </summary>
            public const float DEFAULT_CENTER_THRESHOLD = 0.2f;

            /// <summary>
            /// ITrampoline 오브젝트 Pool의 초기 용량
            /// </summary>
            public const int POOL_DEFAULT_CAPACITY = 20;

            /// <summary>
            /// ITrampoline 오브젝트 Pool의 최대 크기
            /// </summary>
            public const int POOL_MAX_SIZE = 50;

            /// <summary>
            /// 생성된 Trampoline들이 배치될 루트 GameObject 이름
            /// </summary>
            public const string ROOT_NAME = "TrampolineRoot";
        }

        public static class Character
        {
            /// <summary>
            /// Character가 받을 수 있는 최소 튕김 힘
            /// </summary>
            public const float MIN_BOUNCE_FORCE = 5f;

            /// <summary>
            /// Character가 받을 수 있는 최대 튕김 힘
            /// </summary>
            public const float MAX_BOUNCE_FORCE = 30f;

            /// <summary>
            /// 속도가 튕김 힘에 미치는 영향 계수
            /// </summary>
            public const float SPEED_BOUNCE_MULTIPLIER = 0.02f;

            /// <summary>
            /// 반사 방향 계산 시 속도 임계값 (이하일 경우 노말 방향으로 Fallback)
            /// </summary>
            public const float MIN_VELOCITY_FOR_REFLECT = 0.01f;
        }

        public static class Goal
        {
            /// <summary>
            /// 거부된 Egg를 내보내기 전까지 머금는 시간 (초)
            /// </summary>
            public const float REJECT_HOLD_DURATION = 0.4f;

            /// <summary>
            /// 거부된 Egg를 내보내는 힘
            /// </summary>
            public const float REJECT_FORCE = 8f;

            /// <summary>
            /// 내보내는 방향이 수직에서 벗어날 수 있는 최대 각도 (도)
            /// </summary>
            public const float REJECT_CONE_HALF_ANGLE = 40f;
        }

        public static class Score
        {
            public static class Keys
            {
                /// <summary>
                /// ScoreRecord가 ISettingsRepository에 저장될 때 사용되는 키
                /// </summary>
                public const string REPOSITORY_KEY = "ScoreRecord";
            }

            /// <summary>
            /// EggColor가 일치하는 Egg가 도달했을 때 얻는 점수
            /// </summary>
            public const int MATCHED = 1;
        }

        public static class Audio
        {
            public static class Keys
            {
                /// <summary>
                /// AudioSettings가 ISettingsRepository에 저장될 때 사용되는 키
                /// </summary>
                public const string REPOSITORY_KEY = "AudioSettings";
            }

            /// <summary>
            /// 볼륨 관련 상수
            /// </summary>
            public static class Volume
            {
                /// <summary>
                /// 볼륨의 기본값 (최대)
                /// </summary>
                public const float DEFAULT = 1f;

                /// <summary>
                /// 음소거로 간주할 최소 정규화 볼륨 임계값
                /// </summary>
                public const float MIN_NORMALIZED_THRESHOLD = 0.0001f;

                /// <summary>
                /// 음소거 상태의 dB 값.
                /// </summary>
                public const float MUTED_DECIBEL = -80f;

                /// <summary>
                /// 정규화된 볼륨값을 dB로 변환할 때 사용되는 계수. (음향학 표준: 20 * log10(normalizedValue))
                /// </summary>
                public const float DECIBEL_MULTIPLIER = 20f;
            }
        }

        public static class Graphics
        {
            public static class Keys
            {
                /// <summary>
                /// GraphicsSettings가 ISettingsRepository에 저장될 때 사용되는 키
                /// </summary>
                public const string REPOSITORY_KEY = "GraphicsSettings";
            }

            /// <summary>
            /// 목표 프레임레이트 (미설정 시 모바일은 30fps로 고정)
            /// </summary>
            public const int TARGET_FRAME_RATE = 60;

            /// <summary>
            /// 그래픽 품질 수준. Unity Quality Settings 인덱스에 매핑.
            /// </summary>
            public static class Quality
            {
                public const int DEFAULT = MEDIUM;

                public const int LOW = 0;
                public const int MEDIUM = 1;
                public const int HIGH = 2;
            }
        }

        public static class Language
        {
            public static class Keys
            {
                /// <summary>
                /// LanguageSettings가 ISettingsRepository에 저장될 때 사용되는 키
                /// </summary>
                public const string REPOSITORY_KEY = "LanguageSettings";
            }

            /// <summary>
            /// Unity Localization 패키지의 Locale Identifier 코드.
            /// </summary>
            public static class LocaleCode
            {
                public const string DEFAULT = ENGLISH;

                public const string ENGLISH = "en";
            }
        }
    }
}