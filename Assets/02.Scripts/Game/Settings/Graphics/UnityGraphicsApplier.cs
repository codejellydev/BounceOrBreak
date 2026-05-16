using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Settings.Graphics
{
    /// <summary>
    /// Unity의 QualitySettings/Application API 기반 <see cref="IGraphicsSettingsApplier"/> 구현체.
    /// </summary>
    public class UnityGraphicsApplier : IGraphicsSettingsApplier
    {
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
    }
}