using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Loading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Scope
{
    /// <summary>
    /// LoadingScene의 LifetimeScope.
    /// </summary>
    public class LoadingSceneScope : LifetimeScope
    {
        #region Private Variables
        [Header("Loading")]
        [SerializeField] private LoadingView _loadingView;
        #endregion


        #region Protected Methods
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_loadingView);
            builder.RegisterEntryPoint<LoadingPresenter>()
                   .As<ILoadingDisplay>();
        }
        #endregion
    }
}