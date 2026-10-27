namespace CodeJelly.BounceOrBreak.Core
{
    /// <summary>
    /// 애플리케이션 전역 상수를 관리하는 컨테이너 클래스.
    /// 추후 상수가 많아질 때를 대비 partial로 선언.
    /// </summary>
    public static partial class AppConstants
    {
        public static class AppLogger
        {
            /// <summary>
            /// 로그 작성 쓰레드의 최대 대기시간
            /// </summary>
            public const int WAIT_TIMEOUT_MS = 200;

            /// <summary>
            /// 한 번에 파일에 기록할 로그 개수
            /// </summary>
            public const int BATCH_SIZE = 50;

            /// <summary>
            /// 쓰레드 종료 대기 타임아웃 (ms)
            /// </summary>
            public const int SHUTDOWN_TIMEOUT_MS = 2000;
        }

        public static class Viewport
        {
            /// <summary>
            /// 보장 영역의 기본 기준 화면비 (9:16 세로 화면)
            /// </summary>
            public const float DEFAULT_DESIGN_ASPECT = 0.5625f;

            /// <summary>
            /// 기본으로 보장할 최소 화면비 (9:22.5 세로 화면)
            /// </summary>
            public const float DEFAULT_MIN_ASPECT = 0.4f;

            /// <summary>
            /// 화면비와 크기에 허용되는 최솟값
            /// (0 이하 입력 시 계산 발산 방지를 위해 하한으로 사용)
            /// </summary>
            public const float MIN_SETTING_VALUE = 0.01f;

            /// <summary>
            /// ViewportFitter가 driven으로 등록하는 직렬화 프로퍼티 경로
            /// </summary>
            public static class Fitter
            {
                public const string ORTHOGRAPHIC_SIZE_PROPERTY = "orthographic size";
                public const string POSITION_X_PROPERTY = "m_LocalPosition.x";
                public const string POSITION_Y_PROPERTY = "m_LocalPosition.y";
            }
        }

        public static class UI
        {
            /// <summary>
            /// Canvas 배율의 하한. 화면 폭이 설계 폭 미만이면 몫이 0이 되므로 이 값으로 고정
            /// </summary>
            public const int MIN_SCALE_FACTOR = 1;

            /// <summary>
            /// Canvas 배율의 기본값. PixelCanvasScaler가 꺼질 때 되돌림
            /// </summary>
            public const float DEFAULT_SCALE_FACTOR = 1f;

            /// <summary>
            /// Canvas Reference PPU의 기본값. PixelCanvasScaler가 꺼질 때 되돌림
            /// </summary>
            public const float DEFAULT_REFERENCE_PIXELS_PER_UNIT = 100f;
        }

        public static class Scene
        {
            /// <summary>
            /// 로딩 화면의 최소 표시 시간 (초)
            /// </summary>
            public const float MIN_LOADING_SCENE_SECONDS = 0.3f;
        }

        /// <summary>
        /// DrivenProperties가 리플렉션으로 찾는 Unity 내부 타입·메서드 이름
        /// </summary>
        public static class DrivenProperties
        {
            public const string MANAGER_TYPE_NAME = "UnityEngine.DrivenPropertyManager";
            public const string REGISTER_METHOD_NAME = "TryRegisterProperty";
            public const string UNREGISTER_ALL_METHOD_NAME = "UnregisterProperties";
        }

        public static class AppVersion
        {
            /// <summary>
            /// 접미사 구분자. 사전 출시 표기(-)와 빌드 메타데이터(+)
            /// </summary>
            public static readonly char[] SUFFIX_SEPARATORS = { '-', '+' };
        }
    }
}