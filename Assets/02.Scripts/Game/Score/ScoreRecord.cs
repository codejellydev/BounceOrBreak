using System;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Score
{
    /// <summary>
    /// 점수 기록 데이터 모델.
    /// </summary>
    [Serializable]
    public class ScoreRecord
    {
        #region Properties
        /// <summary>
        /// 최고 점수 (0 이상).
        /// </summary>
        public int BestScore
        {
            get => _bestScore;
            set => _bestScore = Mathf.Max(0, value);
        }
        #endregion


        #region Private Variables
        [SerializeField] private int _bestScore;
        #endregion
    }
}
