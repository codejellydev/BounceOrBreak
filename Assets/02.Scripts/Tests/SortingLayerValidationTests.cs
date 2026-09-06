using CodeJelly.BounceOrBreak.Game;
using NUnit.Framework;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// Sorting Layer 상수와 TagManager 설정의 정합성을 검증하는 테스트 클래스.
    /// </summary>
    public class SortingLayerValidationTests
    {
        #region Public Methods
        /// <summary>
        /// 상수로 정의한 모든 Sorting Layer가 프로젝트에 존재합니다.
        /// </summary>
        [Test]
        public void SortingLayers_AllExist()
        {
            foreach (string name in GameConstants.SortingLayers.ORDER)
            {
                // NameToID는 없는 이름에 Default의 id 0을 돌려주므로 0을 부재로 판정
                Assert.AreNotEqual(0, SortingLayer.NameToID(name), $"Sorting Layer '{name}' does not exist.");
            }
        }

        /// <summary>
        /// TagManager의 모든 Sorting Layer가 ORDER에 선언되어 있습니다.
        /// </summary>
        [Test]
        public void SortingLayers_HaveNoUnlistedLayers()
        {
            foreach (SortingLayer layer in SortingLayer.layers)
            {
                // Default는 배정 누락의 표시로만 쓰므로 검사에서 제외
                if (layer.id == 0)
                {
                    continue;
                }

                Assert.Contains(layer.name, GameConstants.SortingLayers.ORDER, $"Sorting Layer '{layer.name}' is not declared in ORDER.");
            }
        }

        /// <summary>
        /// TagManager의 Sorting Layer 순서가 ORDER와 일치합니다.
        /// </summary>
        [Test]
        public void SortingLayers_MatchDeclaredOrder()
        {
            string[] order = GameConstants.SortingLayers.ORDER;

            for (int i = 1; i < order.Length; i++)
            {
                int previous = SortingLayer.GetLayerValueFromID(SortingLayer.NameToID(order[i - 1]));
                int current = SortingLayer.GetLayerValueFromID(SortingLayer.NameToID(order[i]));

                Assert.Less(previous, current, $"'{order[i - 1]}' must be drawn behind '{order[i]}'.");
            }
        }
        #endregion
    }
}
