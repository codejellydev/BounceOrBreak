using System;
using System.Collections.Generic;

namespace CodeJelly.BounceOrBreak.Editor.PixelGrid
{
    /// <summary>
    /// 컴포넌트 타입별 검사 대상 속성.
    /// </summary>
    public readonly struct PixelGridRule
    {
        #region Constructors
        public PixelGridRule(
            Type componentType,
            bool isNegativeUnset,
            params string[] propertyPaths)
        {
            ComponentType = componentType;
            IsNegativeUnset = isNegativeUnset;
            PropertyPaths = propertyPaths;
        }
        #endregion


        #region Properties
        public Type ComponentType { get; }

        /// <summary>
        /// 음수를 미설정으로 간주하는지 여부. true이면 음수 값은 검사에서 제외합니다.
        /// </summary>
        public bool IsNegativeUnset { get; }

        /// <summary>
        /// 검사 대상 속성의 SerializedProperty 경로.
        /// </summary>
        public IReadOnlyList<string> PropertyPaths { get; }
        #endregion
    }
}