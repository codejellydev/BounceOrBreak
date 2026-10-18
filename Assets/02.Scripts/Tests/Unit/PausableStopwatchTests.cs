using CodeJelly.BounceOrBreak.Core.Pause;
using NUnit.Framework;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// PausableStopwatch의 경과 시간 계산을 검증하는 테스트 클래스.
    /// </summary>
    public class PausableStopwatchTests
    {
        #region Private Variables
        private PausableStopwatch _stopwatch;
        #endregion


        #region Public Methods
        [SetUp]
        public void SetUp()
        {
            _stopwatch = new PausableStopwatch();
            _stopwatch.Start(0f);
        }

        /// <summary>
        /// 정지가 없으면 시작 시각부터의 시간이 그대로 경과 시간입니다.
        /// </summary>
        [Test]
        public void GetElapsed_WithoutPause_ReturnsTimeSinceStart()
        {
            Assert.AreEqual(10f, _stopwatch.GetElapsed(10f));
        }

        /// <summary>
        /// 멈춰 있는 동안에는 경과 시간이 늘지 않습니다.
        /// </summary>
        [Test]
        public void GetElapsed_WhilePaused_StopsCounting()
        {
            _stopwatch.Pause(3f);

            Assert.AreEqual(3f, _stopwatch.GetElapsed(10f));
        }

        /// <summary>
        /// 이어가면 정지 구간을 뺀 채 다시 늘어납니다.
        /// </summary>
        [Test]
        public void GetElapsed_AfterResume_ExcludesPausedInterval()
        {
            _stopwatch.Pause(3f);
            _stopwatch.Resume(5f);

            Assert.AreEqual(8f, _stopwatch.GetElapsed(10f));
        }

        /// <summary>
        /// 멈춰 있을 때 다시 멈춰도 정지 시작 시각은 바뀌지 않습니다.
        /// </summary>
        [Test]
        public void Pause_WhilePaused_KeepsFirstPauseStart()
        {
            _stopwatch.Pause(3f);
            _stopwatch.Pause(4f);
            _stopwatch.Resume(5f);

            Assert.AreEqual(8f, _stopwatch.GetElapsed(10f));
        }

        /// <summary>
        /// 멈춰 있지 않을 때 이어가기는 무시됩니다.
        /// </summary>
        [Test]
        public void Resume_WhileRunning_IsIgnored()
        {
            _stopwatch.Resume(5f);

            Assert.AreEqual(10f, _stopwatch.GetElapsed(10f));
        }

        /// <summary>
        /// 멈춰 있을 때 다시 시작해도 멈춘 상태는 유지됩니다.
        /// </summary>
        [Test]
        public void Start_WhilePaused_KeepsPaused()
        {
            _stopwatch.Pause(3f);
            _stopwatch.Start(5f);

            Assert.IsTrue(_stopwatch.IsPaused);
            Assert.AreEqual(0f, _stopwatch.GetElapsed(10f));
        }

        /// <summary>
        /// 다시 시작하면 이전 정지 구간이 지워집니다.
        /// </summary>
        [Test]
        public void Start_ClearsTotalPausedTime()
        {
            _stopwatch.Pause(3f);
            _stopwatch.Resume(5f);
            _stopwatch.Start(10f);

            Assert.AreEqual(4f, _stopwatch.GetElapsed(14f));
        }
        #endregion
    }
}