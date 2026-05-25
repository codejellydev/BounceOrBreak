using System;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Settings.Graphics
{
    /// <summary>
    /// 그래픽 설정 데이터 모델.
    /// </summary>
    [Serializable]
    public class GraphicsSettings
    {
        #region Properties
        /// <summary>
        /// 그래픽 품질 수준 (Unity QualitySettings 인덱스).
        /// </summary>
        public int QualityLevel
        {
            get => _qualityLevel;
            set => _qualityLevel = value;
        }
        #endregion


        #region Private Variables
        [SerializeField] private int _qualityLevel = GameConstants.Graphics.Quality.DEFAULT;
        #endregion
    }
}