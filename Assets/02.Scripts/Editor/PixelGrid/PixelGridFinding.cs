using UnityEngine;

namespace CodeJelly.BounceOrBreak.Editor.PixelGrid
{
    /// <summary>
    /// 도트 격자 검사 결과 데이터.
    /// </summary>
    public readonly struct PixelGridFinding
    {
        #region Constructors
        public PixelGridFinding(
            Component component,
            string location,
            string detail)
        {
            Component = component;
            Location = location;
            Detail = detail;
        }
        #endregion


        #region Properties
        /// <summary>
        /// 해당 값을 가진 컴포넌트. 에셋을 검사한 경우 검사 종료 후에는 유효하지 않습니다.
        /// </summary>
        public Component Component { get; }

        /// <summary>
        /// 루트부터 Component까지의 Hierarchy 경로.
        /// </summary>
        public string Location { get; }

        public string Detail { get; }
        #endregion
    }
}