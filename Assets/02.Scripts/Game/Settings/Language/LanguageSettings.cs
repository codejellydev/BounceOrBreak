using System;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Settings.Language
{
    /// <summary>
    /// 언어 설정 데이터 모델.
    /// </summary>
    [Serializable]
    public class LanguageSettings
    {
        #region Properties
        /// <summary>
        /// 선택된 로케일 식별자 코드.
        /// </summary>
        public string LocaleCode
        {
            get => _localeCode;
            set => _localeCode = value;
        }
        #endregion


        #region Private Variables
        [SerializeField] private string _localeCode = GameConstants.Language.LocaleCode.DEFAULT;
        #endregion
    }
}