using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Core.Pause
{
    /// <summary>
    /// 일시정지를 지원하는 경과 시간 계측기.
    /// </summary>
    public class PausableStopwatch
    {
        #region Properties
        public bool IsPaused { get; private set; }
        #endregion


        #region Private Variables
        private float _startTime;
        private float _pauseStartTime;
        private float _totalPausedTime;
        #endregion


        #region Public Methods
        /// <summary>
        /// 지정한 시각을 기준으로 스톱워치를 시작합니다. 이전 기록은 지워지고 멈춘 상태는 유지됩니다.
        /// </summary>
        public void Start(float now)
        {
            _startTime = now;
            _totalPausedTime = 0f;

            if (IsPaused == true)
            {
                _pauseStartTime = now;
            }
        }

        /// <summary>
        /// 지정한 시각에 스톱워치를 멈춥니다. 이미 멈춰 있으면 무시합니다.
        /// </summary>
        public void Pause(float now)
        {
            if (IsPaused == true)
            {
                return;
            }

            IsPaused = true;
            _pauseStartTime = now;
        }

        /// <summary>
        /// 지정한 시각에 멈춘 스톱워치를 이어갑니다. 멈춰 있지 않으면 무시합니다.
        /// </summary>
        public void Resume(float now)
        {
            if (IsPaused == false)
            {
                return;
            }

            IsPaused = false;
            _totalPausedTime += now - _pauseStartTime;
        }

        /// <summary>
        /// 지정한 시각까지의 경과 시간을 정지 구간을 빼고 반환합니다.
        /// </summary>
        public float GetElapsed(float now)
        {
            float pausedTime = _totalPausedTime;

            if (IsPaused == true)
            {
                pausedTime += now - _pauseStartTime;
            }

            return now - _startTime - pausedTime;
        }

        /// <summary>
        /// 현재 시각을 기준으로 스톱워치를 시작합니다. 이전 기록은 지워지고 멈춘 상태는 유지됩니다.
        /// </summary>
        public void Start()
        {
            Start(Time.time);
        }

        /// <summary>
        /// 현재 시각에 스톱워치를 멈춥니다. 이미 멈춰 있으면 무시합니다.
        /// </summary>
        public void Pause()
        {
            Pause(Time.time);
        }

        /// <summary>
        /// 현재 시각에 멈춘 스톱워치를 이어갑니다. 멈춰 있지 않으면 무시합니다.
        /// </summary>
        public void Resume()
        {
            Resume(Time.time);
        }

        /// <summary>
        /// 현재 시각까지의 경과 시간을 정지 구간을 빼고 반환합니다.
        /// </summary>
        public float GetElapsed()
        {
            return GetElapsed(Time.time);
        }

        /// <summary>
        /// 정지 구간을 뺀 경과 시간이 지정한 시간(초)만큼 흐를 때까지 기다립니다.
        /// </summary>
        public async UniTask WaitForSecondsAsync(float seconds, CancellationToken ct)
        {
            float target = GetElapsed() + seconds;

            while (GetElapsed() < target)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }
        #endregion
    }
}