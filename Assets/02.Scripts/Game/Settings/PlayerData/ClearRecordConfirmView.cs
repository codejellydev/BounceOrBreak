using CodeJelly.BounceOrBreak.Game.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Game.Settings.PlayerData
{
    /// <summary>
    /// 최고 점수 기록 삭제 확인 패널의 View 클래스.
    /// ClearRecordConfirmPresenter로부터 제어됩니다.
    /// </summary>
    public class ClearRecordConfirmView : MonoBehaviour
    {
        #region Properties
        public PushButton ConfirmButton => _confirmButton;
        public PushButton CancelButton => _cancelButton;

        /// <summary>
        /// 패널 외부(Dim 영역) 클릭으로 패널을 닫는 버튼.
        /// </summary>
        public Button DimBackgroundButton => _dimBackgroundButton;

        /// <summary>
        /// View가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _confirmButton != null && _cancelButton != null && _dimBackgroundButton != null;
        #endregion


        #region Private Variables
        [Header("Buttons")]
        [SerializeField] private PushButton _confirmButton;
        [SerializeField] private PushButton _cancelButton;
        [SerializeField] private Button _dimBackgroundButton;
        #endregion
    }
}