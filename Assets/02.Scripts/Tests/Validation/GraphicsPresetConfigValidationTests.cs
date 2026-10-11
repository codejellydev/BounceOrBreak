using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.Settings.Graphics;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests.Validation
{
    /// <summary>
    /// GraphicsPresetConfig 에셋의 유효성을 검증하는 테스트 클래스.
    /// </summary>
    public class GraphicsPresetConfigValidationTests
    {
        #region Private Variables
        private static readonly int[] QUALITY_LEVELS =
        {
            GameConstants.Graphics.Quality.LOW,
            GameConstants.Graphics.Quality.MEDIUM,
            GameConstants.Graphics.Quality.HIGH,
        };
        #endregion


        #region Public Methods
        /// <summary>
        /// 모든 품질 수준에 프리셋이 있고 값이 유효합니다.
        /// </summary>
        [Test]
        public void Presets_CoverEveryQualityLevel()
        {
            List<GraphicsPresetConfig> configs = new List<GraphicsPresetConfig>(EditorAssetLoader.LoadAll<GraphicsPresetConfig>());

            // 에셋을 못 찾으면 무조건 통과하므로 로드 자체를 검증
            Assert.IsNotEmpty(configs, "No GraphicsPresetConfig asset found.");

            foreach (GraphicsPresetConfig config in configs)
            {
                Assert.AreEqual(QUALITY_LEVELS.Length, config.Presets.Length, $"{config.name} must declare one preset per quality level.");

                foreach (int level in QUALITY_LEVELS)
                {
                    Assert.IsTrue(config.TryGetPreset(level, out GraphicsPresetConfig.Preset preset), $"No preset for quality level {level} in {config.name}.");

                    Assert.GreaterOrEqual(preset.TargetFrameRate, GameConstants.Graphics.DEVICE_MAX_FRAME_RATE, $"Negative target frame rate at level {level} in {config.name}.");
                }
            }
        }

        /// <summary>
        /// Unity Quality Settings의 단계 수가 품질 수준 상수의 수와 같습니다.
        /// </summary>
        [Test]
        public void QualitySettings_MatchQualityLevels()
        {
            // 품질 수준 하나가 Unity 단계와 프리셋을 같은 번호로 가리키므로 개수가 같아야 함
            Assert.AreEqual(QUALITY_LEVELS.Length, QualitySettings.names.Length, "Project Settings > Quality must have one level per GameConstants.Graphics.Quality value.");
        }
        #endregion
    }
}