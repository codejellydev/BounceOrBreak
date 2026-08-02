using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Scene;
using Cysharp.Threading.Tasks;
using System.Threading;
using VContainer;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Bootstrap
{
    /// <summary>
    /// SceneLoader를 거치지 않고 단독 실행된 Scene의 초기화를 대행하는 클래스.
    /// SceneLoader가 구동한 경우에는 동작하지 않습니다.
    /// </summary>
    public class StandaloneSceneBootstrapper : IAsyncStartable
    {
        #region Injection
        private readonly AppInitializer _appInitializer;
        private readonly SceneLoader _sceneLoader;
        private readonly IObjectResolver _resolver;

        public StandaloneSceneBootstrapper(
            AppInitializer appInitializer,
            SceneLoader sceneLoader,
            IObjectResolver resolver)
        {
            _appInitializer = appInitializer;
            _sceneLoader = sceneLoader;
            _resolver = resolver;
        }
        #endregion


        #region VContainer Lifecycle
        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            // SceneLoader가 전환 중이라면 초기화는 SceneLoader가 담당
            if (_sceneLoader.IsTransitioning == true)
            {
                return;
            }

            AppLogger.Info("[StandaloneSceneBootstrapper] Standalone scene detected. Bootstrapping...");

            if (await _appInitializer.EnsureInitializedAsync(cancellation) == false)
            {
                AppLogger.Error("[StandaloneSceneBootstrapper] Bootstrap aborted due to system initialization failure.");

                return;
            }

            await InitializeSceneAsync(cancellation);
            ActivateScene();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Scene의 ISceneInitializer가 존재하면 초기화를 수행합니다.
        /// </summary>
        private async UniTask InitializeSceneAsync(CancellationToken ct)
        {
            if (_resolver.TryResolve(out ISceneInitializer initializer) == false)
            {
                return;
            }

            await initializer.InitializeAsync(null, ct);
        }

        /// <summary>
        /// Scene의 ISceneActivatable이 존재하면 활성화를 통지합니다.
        /// </summary>
        private void ActivateScene()
        {
            if (_resolver.TryResolve(out ISceneActivatable activatable) == false)
            {
                return;
            }

            activatable.OnActivated();
        }
        #endregion
    }
}
