using CodeJelly.BounceOrBreak.Game.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Game.Lobby
{
    /// <summary>
    /// LobbyScene의 View 클래스.
    /// LobbyPresenter로부터 제어됩니다.
    /// </summary>
    public class LobbyView : MonoBehaviour
    {
        #region Properties
        public Button StartButton => _startButton;
        public PushButton SettingsButton => _settingsButton;
        #endregion


        #region Private Variables
        [Header("Buttons")]
        [SerializeField] private Button _startButton;
        [SerializeField] private PushButton _settingsButton;
        #endregion
    }
}