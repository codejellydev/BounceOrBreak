using CodeJelly.BounceOrBreak.Core.Localization;
using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Resource;
using CodeJelly.BounceOrBreak.Game.Input;
using CodeJelly.BounceOrBreak.Game.Settings.Graphics;
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
        private readonly IGraphicsSettingsApplier _graphicsApplier;

        public AppInitializer(
            ResourceManager resourceManager,
            InputManager inputManager,
            LocalizationManager localizationManager,
            IGraphicsSettingsApplier graphicsApplier)
        {
            _resourceManager = resourceManager;
            _inputManager = inputManager;
            _localizationManager = localizationManager;
            _graphicsApplier = graphicsApplier;
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

            // 목표 프레임레이트 적용
            _graphicsApplier.ApplyTargetFrameRate(GameConstants.Graphics.TARGET_FRAME_RATE);

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

            // 실패 시에는 플래그를 세우지 않으므로 다음 호출에서 재시도됨
            _isInitialized = true;

            AppLogger.Info("[AppInitializer] All systems initialized.");

            return true;
        }
        #endregion
    }
}
