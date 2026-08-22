using CodeJelly.BounceOrBreak.Core.Viewport;
using NUnit.Framework;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// ViewportContract의 화면비별 Camera 설정 계산을 검증하는 테스트 클래스.
    /// </summary>
    public class ViewportContractTests
    {
        #region Private Variables
        private const float TOLERANCE = 0.0001f;

        // 모든 테스트가 공유하는 ViewportContract. 세로 절반은 가로 절반 / 기준 화면비 (2 / 0.5 = 4)
        private const float HALF_WIDTH = 2f;
        private const float DESIGN_ASPECT = 0.5f;
        private const float DESIGN_HALF_HEIGHT = 4f;
        private const float MIN_ASPECT = 0.4f;

        private const float TALLER_ASPECT = 0.4f;
        private const float WIDER_ASPECT = 0.8f;

        // 모든 화면비를 훑는 테스트의 검사 범위. 보장 최소 화면비 0.4보다 아래부터 가로로 넓은 화면까지 포함
        private const float SWEEP_MIN_ASPECT = 0.3f;
        private const float SWEEP_MAX_ASPECT = 1.5f;
        private const float SWEEP_STEP = 0.1f;
        #endregion


        #region Public Methods
        /// <summary>
        /// 기준 화면비에서는 영역이 화면에 정확히 맞습니다.
        /// </summary>
        [Test]
        public void Resolve_DesignAspect_MatchesDesignHeight()
        {
            ViewportFit fit = CreateContract(ViewportAnchor.Bottom).Resolve(DESIGN_ASPECT);

            Assert.AreEqual(DESIGN_HALF_HEIGHT, fit.OrthographicSize, TOLERANCE);
            Assert.AreEqual(0f, fit.CenterY, TOLERANCE);
        }

        /// <summary>
        /// 기준보다 세로로 긴 화면에서는 늘어난 세로 공간이 위쪽에 배치됩니다.
        /// </summary>
        [Test]
        public void Resolve_NarrowerAspect_ExpandsHeightUpward()
        {
            const float expectedSize = 5f;
            const float expectedCenterY = 1f;

            ViewportFit fit = CreateContract(ViewportAnchor.Bottom).Resolve(TALLER_ASPECT);

            // 가로 기준이 세로 기준보다 커서 채택되고, 늘어난 만큼 위로 이동 (2 / 0.4 = 5, 5 - 4 = 1)
            Assert.AreEqual(expectedSize, fit.OrthographicSize, TOLERANCE);
            Assert.AreEqual(expectedCenterY, fit.CenterY, TOLERANCE);
        }

        /// <summary>
        /// 기준보다 가로로 넓은 화면에서는 세로 절반이 그대로 orthographicSize가 됩니다.
        /// </summary>
        [Test]
        public void Resolve_WiderAspect_KeepsDesignHeight()
        {
            ViewportFit fit = CreateContract(ViewportAnchor.Bottom).Resolve(WIDER_ASPECT);

            Assert.AreEqual(DESIGN_HALF_HEIGHT, fit.OrthographicSize, TOLERANCE);
            Assert.AreEqual(0f, fit.CenterY, TOLERANCE);
        }

        /// <summary>
        /// 화면비가 유효하지 않으면 세로 기준으로만 Camera를 설정합니다.
        /// </summary>
        [Test]
        public void Resolve_InvalidAspect_FallsBackToDesignHeight()
        {
            ViewportFit fit = CreateContract(ViewportAnchor.Bottom).Resolve(0f);

            Assert.AreEqual(DESIGN_HALF_HEIGHT, fit.OrthographicSize, TOLERANCE);
        }

        /// <summary>
        /// Center 앵커는 세로 여백과 무관하게 중앙을 유지합니다.
        /// </summary>
        [Test]
        public void Resolve_CenterAnchor_KeepsCenterY()
        {
            ViewportFit fit = CreateContract(ViewportAnchor.Center).Resolve(TALLER_ASPECT);

            Assert.AreEqual(0f, fit.CenterY, TOLERANCE);
        }

        /// <summary>
        /// 어떤 화면비에서도 영역 전체가 화면 안에 들어옵니다.
        /// </summary>
        [Test]
        public void Resolve_AnyAspect_ContainsDesignArea(
            [Range(SWEEP_MIN_ASPECT, SWEEP_MAX_ASPECT, SWEEP_STEP)] float aspect)
        {
            ViewportFit fit = CreateContract(ViewportAnchor.Bottom).Resolve(aspect);

            // 화면의 세로 절반 = orthographicSize, 가로 절반 = orthographicSize * 화면비
            Assert.GreaterOrEqual(fit.OrthographicSize, DESIGN_HALF_HEIGHT - TOLERANCE);
            Assert.GreaterOrEqual(fit.OrthographicSize * aspect, HALF_WIDTH - TOLERANCE);
        }

        /// <summary>
        /// 화면 상단은 중심 y좌표에서 orthographicSize만큼 위입니다.
        /// </summary>
        [Test]
        public void TopY_IsCenterPlusSize()
        {
            const float expectedTopY = 6f;

            ViewportFit fit = CreateContract(ViewportAnchor.Bottom).Resolve(TALLER_ASPECT);

            // TopY는 CenterY + orthographicSize (1 + 5 = 6)
            Assert.AreEqual(expectedTopY, fit.TopY, TOLERANCE);
        }

        /// <summary>
        /// 기준 화면비가 유효하지 않으면 정사각형 영역으로 정의됩니다.
        /// </summary>
        [Test]
        public void Constructor_InvalidDesignAspect_DefinesSquare()
        {
            ViewportContract contract = new ViewportContract(HALF_WIDTH, 0f, MIN_ASPECT, ViewportAnchor.Bottom);

            Assert.AreEqual(contract.HalfWidth, contract.HalfHeight, TOLERANCE);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 공유 ViewportContract를 지정한 앵커로 생성합니다.
        /// </summary>
        private static ViewportContract CreateContract(ViewportAnchor anchor)
        {
            return new ViewportContract(
                halfWidth: HALF_WIDTH,
                designAspect: DESIGN_ASPECT,
                minAspect: MIN_ASPECT,
                anchor: anchor);
        }
        #endregion
    }
}
