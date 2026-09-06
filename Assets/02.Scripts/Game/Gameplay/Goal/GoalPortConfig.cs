using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Goal
{
    /// <summary>
    /// GoalPort의 설정을 관리하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "GoalPortConfig", menuName = "Configs/Game/Goal Port Config")]
    public class GoalPortConfig : ScriptableObject
    {
        #region Nested Classes
        /// <summary>
        /// EggColor와 해당 스프라이트의 매핑 단위.
        /// </summary>
        [Serializable]
        private class Entry
        {
            #region Properties
            public EggColor Color => _color;

            public Sprite Sprite => _sprite;
            #endregion


            #region Private Variables
            [Tooltip("포트가 요구하는 EggColor")]
            [SerializeField] private EggColor _color;

            [Tooltip("해당 EggColor 포트의 스프라이트")]
            [SerializeField] private Sprite _sprite;
            #endregion
        }
        #endregion


        #region Private Variables
        [Tooltip("EggColor별 스프라이트 매핑 목록")]
        [SerializeField] private List<Entry> _entries = new List<Entry>();
        #endregion


        #region Public Methods
        /// <summary>
        /// 지정된 EggColor에 해당하는 스프라이트를 반환합니다.
        /// </summary>
        /// <returns>스프라이트. 등록되지 않은 색이면 null</returns>
        public Sprite GetSprite(EggColor color)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Color == color)
                {
                    return _entries[i].Sprite;
                }
            }

            AppLogger.Warning($"[GoalPortConfig] Sprite not found for color: {color}");
            return null;
        }
        #endregion
    }
}
