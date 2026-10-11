using UnityEngine;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Entry
{
    /// <summary>
    /// EntryScene의 Presenter 클래스.
    /// </summary>
    public class EntryPresenter : IStartable
    {
        #region Injection
        private readonly EntryView _view;

        public EntryPresenter(EntryView view)
        {
            _view = view;
        }
        #endregion


        #region VContainer Lifecycle
        public void Start()
        {
            _view.SetVersion(string.Format(GameConstants.Entry.VERSION_FORMAT, Application.version));
        }
        #endregion
    }
}