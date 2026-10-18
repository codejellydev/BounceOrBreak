using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.UI.Panel
{
    /// <summary>
    /// 씬에 미리 놓인 패널 인스턴스임을 표시하는 마커 컴포넌트.
    /// </summary>
    public class UIPanelInstance : MonoBehaviour
    {
        #region Properties
        public string Key => _key;
        #endregion


        #region Private Variables
        [Tooltip("UIPanelConfig에 선언된 패널 키")]
        [SerializeField] private string _key;
        #endregion
    }
}