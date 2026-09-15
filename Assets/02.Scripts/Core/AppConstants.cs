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

        public static class Resource
        {
            /// <summary>
            /// ResourceNode 오브젝트 Pool의 초기 용량
            /// (Addressables Group Key의 예상 개수)
            /// </summary>
            public const int POOL_DEFAULT_CAPACITY = 30;

            /// <summary>
            /// ResourceNode 오브젝트 Pool의 최대 크기
            /// </summary>
            public const int POOL_MAX_SIZE = 500;
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
        }
    }
}