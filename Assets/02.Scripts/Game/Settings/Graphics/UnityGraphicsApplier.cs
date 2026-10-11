using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Settings.Graphics
{
    /// <summary>
    /// Unity의 QualitySettings/Application API 기반 <see cref="IGraphicsSettingsApplier"/> 구현체.
    /// </summary>
    public class UnityGraphicsApplier : IGraphicsSettingsApplier
    {
        #region Injection
        private readonly GraphicsPresetConfig _presetConfig;

        public UnityGraphicsApplier(GraphicsPresetConfig presetConfig)
        {
            _presetConfig = presetConfig;
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// IGraphicsSettingsApplier.ApplyQualityLevel Implementation
        /// </summary>
        public void ApplyQualityLevel(int qualityLevel)
        {
            var names = QualitySettings.names;

            if (qualityLevel < 0 || qualityLevel >= names.Length)
            {
                AppLogger.Error($"[UnityGraphicsApplier] Invalid quality level: {qualityLevel}. Available count: {names.Length}. Check Project Settings - Quality platform checkboxes.");
                return;
            }

            // applyExpensiveChanges=true: 쉐이더/텍스처 재로드까지 즉시 반영.
            QualitySettings.SetQualityLevel(qualityLevel, applyExpensiveChanges: true);
            AppLogger.Info($"[UnityGraphicsApplier] Quality level applied: index={qualityLevel}, name='{names[qualityLevel]}', current={QualitySettings.GetQualityLevel()}");

            if (_presetConfig.TryGetPreset(qualityLevel, out GraphicsPresetConfig.Preset preset) == false)
            {
                AppLogger.Error($"[UnityGraphicsApplier] No graphics preset for quality level {qualityLevel}.");
                return;
            }

            ApplyTargetFrameRate(preset.TargetFrameRate);
        }

        /// <summary>
        /// IGraphicsSettingsApplier.ApplyTargetFrameRate Implementation
        /// </summary>
        public void ApplyTargetFrameRate(int targetFrameRate)
        {
            if (targetFrameRate == GameConstants.Graphics.DEVICE_MAX_FRAME_RATE)
            {
                targetFrameRate = ResolveDeviceFrameRate();
            }

            if (targetFrameRate <= 0)
            {
                AppLogger.Error($"[UnityGraphicsApplier] Invalid target frame rate: {targetFrameRate}.");
                return;
            }

            Application.targetFrameRate = targetFrameRate;
            AppLogger.Info($"[UnityGraphicsApplier] Target frame rate applied: {targetFrameRate}");
        }

        /// <summary>
        /// IGraphicsSettingsApplier.ApplySettings Implementation
        /// </summary>
        public void ApplySettings(GraphicsSettings settings)
        {
            if (settings == null)
            {
                AppLogger.Warning("[UnityGraphicsApplier] Attempted to apply null GraphicsSettings.");
                return;
            }

            ApplyQualityLevel(settings.QualityLevel);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 기기의 화면 주사율을 목표 프레임레이트로 환산합니다.
        /// </summary>
        /// <returns>주사율을 반올림한 값. 읽지 못하면 FALLBACK_FRAME_RATE</returns>
        private static int ResolveDeviceFrameRate()
        {
            int refreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);

            return refreshRate > 0 ? refreshRate : GameConstants.Graphics.FALLBACK_FRAME_RATE;
        }
        #endregion
    }
}