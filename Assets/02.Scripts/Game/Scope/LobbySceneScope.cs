using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Lobby;
using UnityEngine;
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
        /// </summary>
        [SerializeField] private SceneTransitionConfig _lobbySceneConfig;
        [SerializeField] private LobbyView _lobbyView;
        #endregion


        #region Protected Methods
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_lobbySceneConfig);
            builder.RegisterComponent(_lobbyView);

            builder.RegisterEntryPoint<LobbyPresenter>();
        }
        #endregion
    }
}
