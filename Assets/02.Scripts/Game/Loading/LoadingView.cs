using CodeJelly.BounceOrBreak.Game.UI.Widgets;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Loading
{
    /// <summary>
    /// LoadingScene의 View 클래스.
    /// LoadingPresenter로부터 제어됩니다.
    /// </summary>
    public class LoadingView : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// LoadingView의 유효 여부.
        /// </summary>
        public bool IsValid => _progressBar != null && _progressBar.IsValid;
        #endregion


        #region Private Variables
        [SerializeField] private ProgressBar _progressBar;
        #endregion


        #region Public Methods
        /// <summary>
        /// 진행률을 ProgressBar에 반영합니다.
        /// </summary>
        public void UpdateProgress(float normalizedValue)
        {
            _progressBar.SetProgress(normalizedValue);
        }
        #endregion
    }
}