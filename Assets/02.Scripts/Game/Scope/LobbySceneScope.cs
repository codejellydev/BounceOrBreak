using CodeJelly.BounceOrBreak.Core.Scene;
using VContainer;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Scope
{
    /// <summary>
    /// LobbyScene의 LifetimeScope.
    /// </summary>
    public class LobbySceneScope : LifetimeScope
    {
        #region Private Variables
        /// <summary>
        /// LobbyScene에서 사용하는 Transition 설정.
        /// </summary>/
        private SceneTransitionConfig _lobbySceneConfig;
        #endregion


        #region Protected Methods
        protected override void Configure(IContainerBuilder builder)
        {
        }
        #endregion
    }
}
