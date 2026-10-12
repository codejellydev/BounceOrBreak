using CodeJelly.BounceOrBreak.Core.Logging;
using R3;
using System;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Score
{
    /// <summary>
    /// 라운드 점수를 관리하는 클래스.
    /// </summary>
    public class RoundScoreService : IDisposable
    {
        #region Properties
        public ReadOnlyReactiveProperty<int> CurrentScore => _currentScore.ToReadOnlyReactiveProperty();
        #endregion


        #region Private Variables
        private readonly ReactiveProperty<int> _currentScore = new ReactiveProperty<int>(0);
        #endregion


        #region Public Methods
        /// <summary>
        /// 득점을 현재 점수에 반영합니다.
        /// </summary>
        public void AddScore()
        {
            _currentScore.Value += GameConstants.Score.MATCHED;
            AppLogger.Debug($"[RoundScoreService] Current score: {_currentScore.Value}");
        }

        /// <summary>
        /// 현재 점수를 초기화합니다.
        /// </summary>
        public void ResetScore()
        {
            _currentScore.Value = 0;
            AppLogger.Debug("[RoundScoreService] Score reset.");
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _currentScore.Dispose();
        }
        #endregion
    }
}