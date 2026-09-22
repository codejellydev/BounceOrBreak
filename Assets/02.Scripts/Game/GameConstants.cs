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

        public static class UI
        {
            /// <summary>
            /// UIPanelConfig에 선언된 패널 키
            /// </summary>
            public static class Keys
            {
                public const string QUIT_CONFIRM = "QuitConfirm";
                public const string PAUSE = "Pause";
                public const string SETTINGS = "Settings";
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
            }

            /// <summary>
            /// Wave 하나가 담당하는 기본 스폰 횟수
            /// </summary>
            public const int DEFAULT_WAVE_SPAWN_COUNT = 10;

            /// <summary>
            /// Wave의 기본 스폰 간격 (초)
            /// </summary>
            public const float DEFAULT_WAVE_SPAWN_INTERVAL = 3f;

            /// <summary>
            /// 마지막 Wave 이후 스폰마다 줄어드는 기본 간격 (초)
            /// </summary>
            public const float DEFAULT_INTERVAL_DECAY_PER_SPAWN = 0.03f;

            /// <summary>
            /// 기본 스폰 간격 하한 (초)
            /// </summary>
            public const float DEFAULT_MIN_SPAWN_INTERVAL = 1.3f;

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
            /// 드로잉이 허용되는 좌우 경계의 x좌표 (Wall의 안쪽 면 = 보장 영역 가로 절반 2.5 - Wall 폭 절반 0.1875)
            /// </summary>
            public const float DRAW_BOUNDARY_X = 2.125f;

            /// <summary>
            /// 라인에 저장할 수 있는 최대 포인트 개수
            /// </summary>
            public const int MAX_LINE_POINTS = 100;

            /// <summary>
            /// Trampoline 생성에 필요한 최소 포인트 개수
            /// </summary>
            public const int MIN_DRAW_POINTS = 2;

            /// <summary>
            /// 기본 두께 (4px / PPU 32)
            /// </summary>
            public const float DEFAULT_THICKNESS = 0.125f;

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
            /// 동시에 존재할 수 있는 최대 드로잉 Trampoline 개수 (스테이지 Trampoline 제외)
            /// </summary>
            public const int MAX_DRAWN_COUNT = 3;

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

            public static class Abbreviation
            {
                /// <summary>
                /// 이 점수부터 단위를 붙여 축약 표시
                /// </summary>
                public const int THRESHOLD = 10000;

                /// <summary>
                /// 축약에 쓰는 단위. 큰 단위부터 나열
                /// </summary>
                public static readonly (int Divisor, string Suffix)[] UNITS =
                {
                    (1_000_000_000, "B"),
                    (1_000_000, "M"),
                    (1_000, "K")
                };
            }
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

        /// <summary>
        /// Sorting Layer 이름.
        /// </summary>
        public static class SortingLayers
        {
            public const string BACKGROUND = "Background";
            public const string TRAMPOLINE = "Trampoline";
            public const string DRAWING = "Drawing";
            public const string EGG = "Egg";
            public const string GOAL_PORT = "GoalPort";
            public const string FOREGROUND = "Foreground";

            /// <summary>
            /// 뒤에서 앞으로의 그리기 순서. TagManager의 리스트 순서와 같아야 함
            /// <para>TagManager와의 정합성은 SortingLayerValidationTests가 검증</para>
            /// </summary>
            public static readonly string[] ORDER =
            {
                BACKGROUND, TRAMPOLINE, DRAWING, EGG, GOAL_PORT, FOREGROUND
            };
        }
    }
}
