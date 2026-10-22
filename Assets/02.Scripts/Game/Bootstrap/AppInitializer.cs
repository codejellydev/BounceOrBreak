using CodeJelly.BounceOrBreak.Core.Localization;
using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Resource;
using CodeJelly.BounceOrBreak.Game.Input;
using CodeJelly.BounceOrBreak.Game.Remote;
using CodeJelly.BounceOrBreak.Game.Score;
using CodeJelly.BounceOrBreak.Game.Settings.Audio;
using CodeJelly.BounceOrBreak.Game.Settings.Graphics;
using CodeJelly.BounceOrBreak.Game.Settings.Language;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace CodeJelly.BounceOrBreak.Game.Bootstrap
{
    /// <summary>
    /// 애플리케이션 필수 시스템의 초기화를 담당하는 클래스.
    /// 씬 진입 경로와 무관하게 단 한 번만 초기화되도록 보장합니다.
    /// </summary>
    public class AppInitializer
    {
        #region Injection
        private readonly ResourceManager _resourceManager;
        private readonly InputManager _inputManager;
        private readonly LocalizationManager _localizationManager;
        private readonly AudioSettingsService _audioSettingsService;
        private readonly GraphicsSettingsService _graphicsSettingsService;
        private readonly LanguageSettingsService _languageSettingsService;
        private readonly ScoreRecordService _scoreRecordService;
        private readonly RemoteConfigService _remoteConfigService;
        private readonly RemoteNoticeService _remoteNoticeService;

        public AppInitializer(
            ResourceManager resourceManager,
            InputManager inputManager,
            LocalizationManager localizationManager,
            AudioSettingsService audioSettingsService,
            GraphicsSettingsService graphicsSettingsService,
            LanguageSettingsService languageSettingsService,
            ScoreRecordService scoreRecordService,
            RemoteConfigService remoteConfigService,
            RemoteNoticeService remoteNoticeService)
        {
            _resourceManager = resourceManager;
            _inputManager = inputManager;
            _localizationManager = localizationManager;
            _audioSettingsService = audioSettingsService;
            _graphicsSettingsService = graphicsSettingsService;
            _languageSettingsService = languageSettingsService;
            _scoreRecordService = scoreRecordService;
            _remoteConfigService = remoteConfigService;
            _remoteNoticeService = remoteNoticeService;
        }
        #endregion


        #region Private Variables
        private bool _isInitialized;
        #endregion


        #region Public Methods
        /// <summary>
        /// 필수 시스템이 초기화되었음을 보장합니다.
        /// 이미 초기화된 경우 즉시 반환하므로 중복 호출해도 안전합니다.
        /// </summary>
        /// <returns>초기화 성공 시 true</returns>
        public async UniTask<bool> EnsureInitializedAsync(CancellationToken ct = default)
        {
            if (_isInitialized == true)
            {
                return true;
            }

            // 네트워크 대기가 긴 원격 설정 수신을 먼저 시작해 두고 마지막에 합류. 실패는 초기화 결과에 영향 없음
            UniTask remoteFetch = _remoteConfigService.FetchAsync(ct);

            // 리소스 시스템 초기화
            if (await _resourceManager.InitializeAsync() == false)
            {
                AppLogger.Error("[AppInitializer] Failed to initialize resource system.");

                return false;
            }

            // 입력 시스템 초기화
            if (await _inputManager.InitializeAsync(ct) == false)
            {
                AppLogger.Error("[AppInitializer] Failed to initialize input system.");

                return false;
            }

            // Localization 시스템 초기화
            if (await _localizationManager.InitializeAsync() == false)
            {
                AppLogger.Error("[AppInitializer] Failed to initialize localization system.");

                return false;
            }

            // 저장된 설정 적용. 언어 적용이 Localization을 쓰므로 그 다음에 수행.
            await _audioSettingsService.LoadAsync();
            await _graphicsSettingsService.LoadAsync();
            await _languageSettingsService.LoadAsync();

            await _scoreRecordService.LoadAsync();

            await remoteFetch;
            await _remoteNoticeService.LoadAsync();

            // 실패 시에는 플래그를 세우지 않으므로 다음 호출에서 재시도됨
            _isInitialized = true;

            AppLogger.Info("[AppInitializer] All systems initialized.");

            return true;
        }
        #endregion
    }
}
