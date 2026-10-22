using CodeJelly.BounceOrBreak.Game.UI.Widgets;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Settings.PlayerData
{
    /// <summary>
    /// 플레이어 데이터 카테고리의 View 클래스.
    /// </summary>
    public class PlayerDataView : MonoBehaviour
    {
        #region Properties
        public PushButton ClearRecordButton => _clearRecordButton;
        #endregion


        #region Private Variables
        [Header("Buttons")]
        [SerializeField] private PushButton _clearRecordButton;
        #endregion
    }
}