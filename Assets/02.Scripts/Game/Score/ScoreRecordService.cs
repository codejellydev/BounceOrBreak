using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Settings;
using Cysharp.Threading.Tasks;
using R3;
using System;

namespace CodeJelly.BounceOrBreak.Game.Score
{
    /// <summary>
    /// 저장되는 점수 기록을 관리하는 클래스.
    /// </summary>
    public class ScoreRecordService : IDisposable
    {
        #region Injection
        private readonly ISettingsRepository _repository;

        public ScoreRecordService(ISettingsRepository repository)
        {
            _repository = repository;
        }
        #endregion


        #region Properties
        public ReadOnlyReactiveProperty<int> BestScore => _bestScore.ToReadOnlyReactiveProperty();
        #endregion


        #region Private Variables
        private readonly ReactiveProperty<int> _bestScore = new ReactiveProperty<int>(0);
        #endregion


        #region Public Methods
        /// <summary>
        /// 저장된 최고 점수를 불러옵니다.
        /// 기록이 없거나 불러오기에 실패하면 기본값으로 시작합니다.
        /// </summary>
        public async UniTask LoadAsync()
        {
            var result = await _repository.LoadAsync<ScoreRecord>(GameConstants.Score.Keys.REPOSITORY_KEY);

            if (result.IsSuccess == true)
            {
                _bestScore.Value = result.Value.BestScore;
            }
            // NotFound는 최초 실행의 정상 흐름이므로 그 외 실패만 로그
            else if (result.Error != SettingsResultError.NotFound)
            {
                AppLogger.Warning($"[ScoreRecordService] Failed to load score record: {result.Error}. Using defaults.");
            }

            AppLogger.Info($"[ScoreRecordService] Loaded. Best score: {_bestScore.Value}");
        }

        /// <summary>
        /// 주어진 점수가 최고 점수를 넘으면 갱신합니다.
        /// </summary>
        /// <returns>최고 점수가 갱신되었으면 true</returns>
        public bool TryUpdateBestScore(int score)
        {
            if (score <= _bestScore.Value)
            {
                return false;
            }

            _bestScore.Value = score;
            AppLogger.Info($"[ScoreRecordService] New best score: {_bestScore.Value}");

            return true;
        }

        /// <summary>
        /// 최고 점수를 저장소에 저장합니다.
        /// </summary>
        public async UniTask SaveAsync()
        {
            ScoreRecord record = new ScoreRecord { BestScore = _bestScore.Value };

            var result = await _repository.SaveAsync(GameConstants.Score.Keys.REPOSITORY_KEY, record);

            if (result.IsSuccess == false)
            {
                AppLogger.Error($"[ScoreRecordService] Failed to save score record: {result.Error}");
            }
        }

        /// <summary>
        /// 저장된 기록을 지우고 최고 점수를 기본값으로 되돌립니다.
        /// </summary>
        public async UniTask ClearAsync()
        {
            var result = await _repository.DeleteAsync(GameConstants.Score.Keys.REPOSITORY_KEY);

            // 삭제가 실패하면 메모리도 그대로 둠
            if (result.IsSuccess == false)
            {
                AppLogger.Error($"[ScoreRecordService] Failed to clear score record: {result.Error}");

                return;
            }

            _bestScore.Value = 0;
            AppLogger.Info("[ScoreRecordService] Score record cleared.");
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _bestScore.Dispose();
        }
        #endregion
    }
}