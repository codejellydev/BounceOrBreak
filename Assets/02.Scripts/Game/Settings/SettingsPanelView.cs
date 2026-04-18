using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Game.Settings
{
    /// <summary>
    /// Settings 패널의 View 클래스.
    /// </summary>
    public class SettingsPanelView : MonoBehaviour
    {
        #region Properties
        public Button CloseButton => _closeButton;
        #endregion


        #region Private Variables
        [Header("Panel Root")]
        [SerializeField] private GameObject _panel;

        [Header("Buttons")]
        [SerializeField] private Button _closeButton;
        #endregion


        #region Public Methods
        /// <summary>
        /// 패널의 표시 여부를 설정합니다.
        /// </summary>
        public void SetPanelActive(bool isActive)
        {
            _panel.SetActive(isActive);
        }
        #endregion
    }
}