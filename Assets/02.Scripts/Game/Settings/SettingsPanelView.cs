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
        /// <summary>
        /// 패널을 닫는 버튼.
        /// </summary>
        public Button CloseButton => _closeButton;

        /// <summary>
        /// 패널 외부(Dim 영역) 클릭으로 패널을 닫는 버튼.
        /// </summary>
        public Button DimBackgroundButton => _dimBackgroundButton;
        #endregion


        #region Private Variables
        [Header("Panel Root")]
        [SerializeField] private GameObject _panel;

        [Header("Buttons")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _dimBackgroundButton;
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