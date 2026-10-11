using CodeJelly.BounceOrBreak.Game.Input;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CodeJelly.BounceOrBreak.Tests.Validation
{
    /// <summary>
    /// InputConfig 에셋의 유효성을 검증하는 테스트 클래스.
    /// </summary>
    public class InputConfigValidationTests
    {
        #region Public Methods
        /// <summary>
        /// 모든 InputContext에 Profile이 하나씩 있습니다.
        /// </summary>
        [Test]
        public void Profiles_CoverEveryContext()
        {
            List<InputConfig> configs = new List<InputConfig>(EditorAssetLoader.LoadAll<InputConfig>());

            // 에셋을 못 찾으면 무조건 통과하므로 로드 자체를 검증
            Assert.IsNotEmpty(configs, "No InputConfig asset found.");

            foreach (InputConfig config in configs)
            {
                foreach (InputContext context in Enum.GetValues(typeof(InputContext)))
                {
                    int count = 0;

                    foreach (InputConfig.InputProfile profile in config.Profiles)
                    {
                        if (profile.Context == context)
                        {
                            count++;
                        }
                    }

                    // 없는 Context로 SwitchContext를 부르면 입력이 통째로 꺼짐
                    Assert.AreNotEqual(0, count, $"No profile for {context} in {config.name}.");

                    // Profile이 둘 이상이면 어느 쪽이 적용될지 정해지지 않음
                    Assert.AreEqual(1, count, $"Duplicate profile for {context} in {config.name}.");
                }
            }
        }
        #endregion
    }
}