namespace CodeJelly.BounceOrBreak.Core
{
    /// <summary>
    /// 애플리케이션 전역 상수를 관리하는 컨테이너 클래스.
    /// </summary>
    public static partial class AppConstants
    {
        public static class Logger
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
    }
}