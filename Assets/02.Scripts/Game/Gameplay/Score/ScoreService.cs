using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Settings;
using Cysharp.Threading.Tasks;
using R3;
using System;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Score
{
    /// <summary>
    /// 점수 누적과 최고 점수 기록을 관리하는 클래스.
    /// 득점 여부 판단은 호출 클래스가 처리하며, 점수 산정 규칙만 담당합니다.
    /// </summary>
    public class ScoreService : IDisposable
    {
        #region Injection
        private readonly ISettingsRepository _settingsRepository;

        public ScoreService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }
        #endregion


        #region Properties
        public ReadOnlyReactiveProperty<int> CurrentScore => _currentScore.ToReadOnlyReactiveProperty();

        public ReadOnlyReactiveProperty<int> BestScore => _bestScore.ToReadOnlyReactiveProperty();
        #endregion


        #region Private Variables
        private readonly ReactiveProperty<int> _currentScore = new ReactiveProperty<int>(0);
        private readonly ReactiveProperty<int> _bestScore = new ReactiveProperty<int>(0);
        #endregion


        #region Public Methods
        /// <summary>
        /// 저장된 최고 점수를 불러옵니다.
        /// 기록이 없거나 불러오기에 실패하면 기본값으로 시작합니다.
        /// </summary>
        public async UniTask InitializeAsync()
        {
            var result = await _settingsRepository.LoadAsync<ScoreRecord>(GameConstants.Score.Keys.REPOSITORY_KEY);

            if (result.IsSuccess == true)
            {
                _bestScore.Value = result.Value.BestScore;
            }
            // 저장된 기록이 없는 최초 실행은 정상 흐름이므로 로그를 남기지 않음
            else if (result.Error != SettingsResultError.NotFound)
            {
                AppLogger.Warning($"[ScoreService] Failed to load score record: {result.Error}. Using defaults.");
            }

            AppLogger.Info($"[ScoreService] Initialized. Best score: {_bestScore.Value}");
        }

        /// <summary>
        /// 득점을 현재 점수에 반영합니다.
        /// </summary>
        public void AddScore()
        {
            _currentScore.Value += GameConstants.Score.MATCHED;
            AppLogger.Debug($"[ScoreService] Current score: {_currentScore.Value}");
        }

        /// <summary>
        /// 현재 점수를 초기화합니다. 최고 점수는 유지됩니다.
        /// </summary>
        public void ResetScore()
        {
            _currentScore.Value = 0;
            AppLogger.Debug("[ScoreService] Score reset.");
        }

        /// <summary>
        /// 현재 점수가 최고 점수를 넘으면 갱신합니다.
        /// </summary>
        /// <returns>최고 점수가 갱신되었으면 true</returns>
        public bool TryUpdateBestScore()
        {
            if (_currentScore.Value <= _bestScore.Value)
            {
                return false;
            }

            _bestScore.Value = _currentScore.Value;
            AppLogger.Info($"[ScoreService] New best score: {_bestScore.Value}");

            return true;
        }

        /// <summary>
        /// 최고 점수를 저장소에 저장합니다.
        /// </summary>
        public async UniTask SaveBestScoreAsync()
        {
            ScoreRecord record = new ScoreRecord { BestScore = _bestScore.Value };

            var result = await _settingsRepository.SaveAsync(GameConstants.Score.Keys.REPOSITORY_KEY, record);

            if (result.IsSuccess == false)
            {
                AppLogger.Error($"[ScoreService] Failed to save score record: {result.Error}");
            }
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _currentScore.Dispose();
            _bestScore.Dispose();
        }
        #endregion
    }
}
