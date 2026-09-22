using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Settings;
using Cysharp.Threading.Tasks;

namespace CodeJelly.BounceOrBreak.Game.Settings.Graphics
{
    /// <summary>
    /// 그래픽 설정의 보관·적용·저장을 담당하는 클래스.
    /// </summary>
    public class GraphicsSettingsService
    {
        #region Injection
        private readonly IGraphicsSettingsApplier _applier;
        private readonly ISettingsRepository _repository;

        public GraphicsSettingsService(
            IGraphicsSettingsApplier applier,
            ISettingsRepository repository)
        {
            _applier = applier;
            _repository = repository;
        }
        #endregion


        #region Properties
        /// <summary>
        /// 현재 그래픽 설정.
        /// </summary>
        public GraphicsSettings Settings => _settings;
        #endregion


        #region Private Variables
        private GraphicsSettings _settings = new GraphicsSettings();
        #endregion


        #region Public Methods
        /// <summary>
        /// 저장된 설정을 불러와 적용합니다.
        /// 기록이 없거나 불러오기에 실패하면 기본값으로 시작합니다.
        /// </summary>
        public async UniTask LoadAsync()
        {
            var result = await _repository.LoadAsync<GraphicsSettings>(GameConstants.Graphics.Keys.REPOSITORY_KEY);

            if (result.IsSuccess == true)
            {
                _settings = result.Value;
            }
            // NotFound는 최초 실행의 정상 흐름이므로 그 외 실패만 로그
            else if (result.Error != SettingsResultError.NotFound)
            {
                AppLogger.Warning($"[GraphicsSettingsService] Failed to load graphics settings: {result.Error}. Using defaults.");
            }

            _applier.ApplySettings(_settings);
        }

        /// <summary>
        /// 그래픽 품질 수준을 바꾸고 적용합니다.
        /// </summary>
        public void SetQualityLevel(int qualityLevel)
        {
            if (_settings.QualityLevel == qualityLevel)
            {
                return;
            }

            _settings.QualityLevel = qualityLevel;
            _applier.ApplyQualityLevel(qualityLevel);
        }

        /// <summary>
        /// 현재 설정을 저장합니다.
        /// </summary>
        public async UniTask SaveAsync()
        {
            var result = await _repository.SaveAsync(GameConstants.Graphics.Keys.REPOSITORY_KEY, _settings);

            if (result.IsSuccess == false)
            {
                AppLogger.Error($"[GraphicsSettingsService] Failed to save graphics settings: {result.Error}");
            }
        }
        #endregion
    }
}