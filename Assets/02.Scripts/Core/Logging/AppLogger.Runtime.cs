#if !UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Core.Logging
{
    /// <summary>
    /// AppLogger의 런타임 구현부. (빌드)
    /// 백그라운드 스레드를 통해 비동기적으로 로그를 파일에 기록합니다.
    /// </summary>
    public static partial class AppLogger
    {
        #region Private Variables
        private static ConcurrentQueue<string> _logQueue;
        private static string _logFilePath;

        /// <summary>
        /// StreamWriter는 thread-safe하지 않지만 현재 코드에서는 단일 쓰레드에서만 접근하기 때문에 동기화 문제 없음.
        /// 추후 멀티쓰레드 Write가 필요한 경우 thread-safe한 래퍼 도입 검토 필요.
        /// </summary>
        private static StreamWriter _fileWriter;
        private static Thread _writeThread;
        private static AutoResetEvent _writeARE;
        private static CancellationTokenSource _cts;
        #endregion

        static AppLogger()
        {
            InitializeFileLogging();

            // Unity 로그 메세지 수신 시 발생하는 이벤트. StackTracing을 위해 연동.
            // (logMessageReceived: 메인 쓰레드 / logMessageReceivedThreaded: 멀티 쓰레드)
            Application.logMessageReceivedThreaded += OnUnityLog;
        }

        /// <summary>
        /// 실제 로그 처리를 수행하는 내부 메서드.
        /// 빌드에서는 파일에 저장합니다.
        /// </summary>
        static partial void LogInternal(LogLevel level, string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                string formattedMessage = $"[{timestamp}] [{level}] {message}";

                // 빌드에서는 Info 이상만 파일 저장
                if (level >= LogLevel.Info)
                {
                    _logQueue.Enqueue(formattedMessage);
                    _writeARE.Set();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }


        #region Private Methods
        /// <summary>
        /// 파일 로깅을 위한 환경 설정을 수행합니다.
        /// </summary>
        private static void InitializeFileLogging()
        {
            // 로그 디렉토리 생성 (Application.persistentDataPath는 플랫폼별로 자동 설정)
            string logDir = Path.Combine(Application.persistentDataPath, "Logs");
            if (Directory.Exists(logDir) == false)
            {
                Directory.CreateDirectory(logDir);
            }

            string fileName = $"BounceOrBreak_{DateTime.Now:yyyyMMdd_HHmmss}.log";
            _logFilePath = Path.Combine(logDir, fileName);

            _fileWriter = new StreamWriter(_logFilePath, append: true, Encoding.UTF8)
            {
                AutoFlush = false // 배치 처리를 위한 설정
            };

            _logQueue = new ConcurrentQueue<string>();
            _writeARE = new AutoResetEvent(false);
            _cts = new CancellationTokenSource();

            _writeThread = new Thread(() => ProcessLogQueue(_cts.Token))
            {
                IsBackground = true,
                Name = "LogWriter",
                Priority = System.Threading.ThreadPriority.BelowNormal
            };
            _writeThread.Start();

            Application.quitting += Shutdown;
        }

        /// <summary>
        /// Queue에서 로그를 꺼내 배치 단위로 파일에 기록합니다.
        /// 백그라운드 쓰레드에서 실행됩니다.
        /// </summary>
        private static void ProcessLogQueue(CancellationToken token)
        {
            while (token.IsCancellationRequested == false || _logQueue.Count > 0)
            {
                _writeARE.WaitOne(AppConstants.AppLogger.WAIT_TIMEOUT_MS);

                int count = 0;
                while (count < AppConstants.AppLogger.BATCH_SIZE && _logQueue.TryDequeue(out string log) == true)
                {
                    try
                    {
                        _fileWriter.WriteLine(log);
                        count++;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }

                if (count > 0)
                {
                    try
                    {
                        _fileWriter.Flush();
                    }
                    catch (Exception)
                    {
                        // Flush 실패 시 무시 (다음 배치에서 재시도)
                    }
                }
            }
        }

        /// <summary>
        /// Unity 로그 콜백 핸들러. 로그를 가공하여 Queue에 적재합니다.
        /// </summary>
        private static void OnUnityLog(string condition, string stackTrace, LogType type)
        {
            string formattedLog = $"[{DateTime.Now:HH:mm:ss}] [Unity-{type}] {condition}";
            if (string.IsNullOrEmpty(stackTrace) == false && type == LogType.Exception)
            {
                formattedLog += $"\n{stackTrace}";
            }
            _logQueue?.Enqueue(formattedLog);
            _writeARE?.Set();
        }

        /// <summary>
        /// 애플리케이션 종료 시 호출되는 정리 메서드.
        /// 백그라운드 쓰레드를 안전하게 종료하고 남은 로그를 파일에 기록합니다.
        /// </summary>
        private static void Shutdown()
        {
            if (_cts == null)
            {
                return;
            }

            _cts.Cancel();
            _writeARE?.Set();

            if (_writeThread?.Join(AppConstants.AppLogger.SHUTDOWN_TIMEOUT_MS) == false)
            {
                int remainingCount = _logQueue?.Count ?? 0;
                UnityEngine.Debug.LogWarning(
                    $"AppLogger thread did not finish within {AppConstants.AppLogger.SHUTDOWN_TIMEOUT_MS}ms. " +
                    $"{remainingCount} logs remaining in queue."
                );
            }

            // 쓰레드 종료 후 잔여 로그 처리
            while (_logQueue?.TryDequeue(out string log) == true)
            {
                try
                {
                    _fileWriter?.WriteLine(log);
                }
                catch
                {
                    // 종료 시점이므로 에러 무시
                }
            }

            _fileWriter?.Flush();
            _fileWriter?.Dispose();
            _cts?.Dispose();
            _writeARE?.Dispose();
        }
        #endregion
    }
}
#endif