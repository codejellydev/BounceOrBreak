using CodeJelly.BounceOrBreak.Core.Viewport;
using NUnit.Framework;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// SafeAreaFit의 앵커 계산 결과를 검증하는 테스트 클래스.
    /// </summary>
    public class SafeAreaFitTests
    {
        #region Private Variables
        private const float TOLERANCE = 0.0001f;

        /// <summary>
        /// 모든 테스트가 공유하는 화면 크기. 100px = 5%가 되도록 1000 x 2000
        /// </summary>
        private const float SCREEN_WIDTH = 1000f;
        private const float SCREEN_HEIGHT = 2000f;
        #endregion


        #region Public Methods
        /// <summary>
        /// safeArea가 화면 전체면 앵커도 화면 전체입니다.
        /// </summary>
        [Test]
        public void Resolve_FullScreen_UsesWholeArea()
        {
            SafeAreaFit fit = SafeAreaFit.Resolve(new Rect(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT), GetScreenSize());

            AssertAnchors(fit, Vector2.zero, Vector2.one);
        }

        /// <summary>
        /// 상단 컷아웃은 anchorMax.y만 낮춥니다.
        /// </summary>
        [Test]
        public void Resolve_TopCutout_LowersAnchorMaxY()
        {
            // 위 100px이 잘리면 남는 높이는 1900 (1900 / 2000 = 0.95)
            SafeAreaFit fit = SafeAreaFit.Resolve(new Rect(0f, 0f, SCREEN_WIDTH, 1900f), GetScreenSize());

            AssertAnchors(fit, Vector2.zero, new Vector2(1f, 0.95f));
        }

        /// <summary>
        /// 하단 제스처 바는 anchorMin.y만 높입니다.
        /// </summary>
        [Test]
        public void Resolve_BottomInset_RaisesAnchorMinY()
        {
            // 아래 100px이 잘리면 시작이 100, 높이 1900 (100 / 2000 = 0.05)
            SafeAreaFit fit = SafeAreaFit.Resolve(new Rect(0f, 100f, SCREEN_WIDTH, 1900f), GetScreenSize());

            AssertAnchors(fit, new Vector2(0f, 0.05f), Vector2.one);
        }

        /// <summary>
        /// 화면 밖으로 나간 safeArea는 앵커 범위로 고정됩니다.
        /// </summary>
        [Test]
        public void Resolve_SafeAreaBeyondScreen_ClampsToAnchorRange()
        {
            SafeAreaFit fit = SafeAreaFit.Resolve(new Rect(-100f, -100f, SCREEN_WIDTH + 200f, SCREEN_HEIGHT + 200f), GetScreenSize());

            AssertAnchors(fit, Vector2.zero, Vector2.one);
        }

        /// <summary>
        /// 화면 크기가 유효하지 않으면 화면 전체를 씁니다.
        /// </summary>
        [Test]
        public void Resolve_InvalidScreenSize_UsesWholeArea()
        {
            SafeAreaFit fit = SafeAreaFit.Resolve(new Rect(0f, 100f, SCREEN_WIDTH, 1900f), Vector2.zero);

            AssertAnchors(fit, Vector2.zero, Vector2.one);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 공유 화면 크기를 반환합니다.
        /// </summary>
        private static Vector2 GetScreenSize()
        {
            return new Vector2(SCREEN_WIDTH, SCREEN_HEIGHT);
        }

        /// <summary>
        /// AnchorMin과 AnchorMax를 기대값과 성분별로 비교합니다.
        /// </summary>
        private static void AssertAnchors(SafeAreaFit fit, Vector2 expectedMin, Vector2 expectedMax)
        {
            Assert.AreEqual(expectedMin.x, fit.AnchorMin.x, TOLERANCE, "AnchorMin.x");
            Assert.AreEqual(expectedMin.y, fit.AnchorMin.y, TOLERANCE, "AnchorMin.y");
            Assert.AreEqual(expectedMax.x, fit.AnchorMax.x, TOLERANCE, "AnchorMax.x");
            Assert.AreEqual(expectedMax.y, fit.AnchorMax.y, TOLERANCE, "AnchorMax.y");
        }
        #endregion
    }
}
