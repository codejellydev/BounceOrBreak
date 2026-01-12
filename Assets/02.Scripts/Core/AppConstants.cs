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
            /// ResourceNode 오브젝트 풀의 초기 용량
            /// (Addressables Group Key의 예상 개수)
            /// </summary>
            public const int POOL_DEFAULT_CAPACITY = 50;

            /// <summary>
            /// ResourceNode 오브젝트 풀의 최대 크기
            /// </summary>
            public const int POOL_MAX_POOL_SIZE = 500;
        }
    }
}