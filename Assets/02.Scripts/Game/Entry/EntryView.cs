using TMPro;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Entry
{
    /// <summary>
    /// EntryScene의 View 클래스.
    /// EntryPresenter로부터 제어됩니다.
    /// </summary>
    public class EntryView : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// EntryView가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _versionLabel != null;
        #endregion


        #region Private Variables
        [SerializeField] private TextMeshProUGUI _versionLabel;
        #endregion


        #region Public Methods
        /// <summary>
        /// 버전 문구를 표시합니다.
        /// </summary>
        public void SetVersion(string version)
        {
            _versionLabel.text = version;
        }
        #endregion
    }
}