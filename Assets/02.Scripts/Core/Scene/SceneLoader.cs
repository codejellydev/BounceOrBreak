using CodeJelly.BounceOrBreak.Core.Logging;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Core.Scene
{
    /// <summary>
    /// Addressables + VContainer 기반 Scene 로딩 클래스.
    /// SceneManager와 혼용을 권장하지 않습니다.
    /// (권장 구조: Unity Build Settings에는 Entry Scene만 포함하고, 나머지는 모두 Addressables로 관리)
    /// Target Scene은 경량으로 유지하고, 무거운 Asset은 ISceneInitializer에서 비동기 로드합니다.
    ///
    /// TODO: Scene key enum 자동 생성 및 빌드 프로세스에서의 키 검증 도입
    /// </summary>
    public class SceneLoader : IDisposable
    {
        #region Nested Classes
        /// <summary>
        /// 타겟 Scene에서 resolve된 컴포넌트 집합.
        /// </summary>
        private readonly struct SceneComponents
        {
            public ISceneInitializer Initializer { get; }
            public ISceneContext Context { get; }
            public ISceneActivatable Activatable { get; }

            public SceneComponents(
                ISceneInitializer initializer,
                ISceneContext context,
                ISceneActivatable activatable)
            {
                Initializer = initializer;
                Context = context;
                Activatable = activatable;
            }
        }
        #endregion


        #region Injection
        private readonly LifetimeScope _parentScope;

        public SceneLoader(LifetimeScope parentScope)
        {
            _parentScope = parentScope;
            _transitionProgress = new ReactiveProperty<float>(0f);

            IsTransitioning = false;
        }
        #endregion


        #region Properties
        /// <summary>
        /// 현재 전환 진행률 (0.0 ~ 1.0)
        /// UI 바인딩에 주로 쓰일 수 있으므로 ReactiveProperty로 선언.
        /// </summary>
        public ReadOnlyReactiveProperty<float> TransitionProgress =>
            _transitionProgress.ToReadOnlyReactiveProperty();

        /// <summary>
        /// 현재 Scene 전환 중인지 여부
        /// </summary>
        public bool IsTransitioning { get; private set; }
        #endregion


        #region Private Variables
        private readonly ReactiveProperty<float> _transitionProgress;

        private SceneInstance _previousSceneInstance;
        private SceneInstance _loadingSceneInstance;

        private CancellationTokenSource _transitionCts;
        #endregion


        #region Public Methods
        /// <summary>
        /// Scene을 비동기적으로 전환합니다.
        /// 초기화 로직, Loading Scene, Payload 전달 등이 필요한 경우 사용합니다.
        /// </summary>
        /// <param name="config">Scene 전환 설정</param>
        /// <param name="payload">Scene에 전달할 데이터</param>
        /// <param name="ct">취소 토큰</param>
        public async UniTask LoadSceneAsync(
            SceneTransitionConfig config,
            IScenePayload payload = null,
            CancellationToken ct = default)
        {
            // Config 유효성 검사
            if (ValidateConfig(config) == false)
            {
                return;
            }

            // 중복 요청 방지
            if (IsTransitioning == true)
            {
                AppLogger.Warning("[SceneLoader] Transition already in progress. Request ignored.");
                return;
            }

            IsTransitioning = true;
            _transitionProgress.Value = 0f;

            // 이전 CTS 정리 (정상 흐름에서는 이미 정리됐지만 안전장치)
            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            _transitionCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            try
            {
                await TransitionSceneAsync(config, payload, _transitionCts.Token);
            }
            catch (OperationCanceledException)
            {
                AppLogger.Info($"[SceneLoader] Transition to {config.TargetSceneKey} was canceled.");
                throw; // 취소 exception은 상위 호출 스택으로 전파
            }
            catch (NullReferenceException ex)
            {
                AppLogger.Exception(ex, $"[SceneLoader] NullReferenceException during transition to '{config.TargetSceneKey}'. " +
                                        $"previousScene.IsValid={_previousSceneInstance.Scene.IsValid()}, " +
                                        $"useLoadingScene={config.UseLoadingScene}");
                throw;
            }
            catch (Exception ex)
            {
                AppLogger.Exception(ex);
                throw; // 상위 호출 스택으로 전파
            }
            finally
            {
                IsTransitioning = false;
                _transitionProgress.Value = 0f;

                _transitionCts?.Dispose();
                _transitionCts = null;
            }
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _transitionCts?.Cancel();
            _transitionCts?.Dispose();

            _transitionProgress.Dispose();

            AppLogger.Info("[SceneLoader] Disposed cleanly.");
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 실제 Scene 전환 로직을 실행합니다.
        /// </summary>
        private async UniTask TransitionSceneAsync(
            SceneTransitionConfig config,
            IScenePayload payload,
            CancellationToken ct)
        {
            SceneInstance targetSceneInstance = default;
            bool transitionSucceeded = false;

            try
            {
                // 1. Loading Scene 로드 (필요 시)
                if (config.UseLoadingScene == true)
                {
                    _loadingSceneInstance = await LoadLoadingSceneAsync(config.LoadingSceneKey, ct);
                }

                // 2. 타겟 Scene 로드 (진행률: 0~50%)
                targetSceneInstance = await LoadTargetSceneAsync(
                    config.TargetSceneKey,
                    MapProgress(0f, 0.5f),
                    ct);

                // 3. SceneScope에서 Scene 컴포넌트 resolve
                SceneComponents components = ResolveSceneComponents(targetSceneInstance);

                // 4. Context 설정
                ApplyPayload(components.Context, payload, config.TargetSceneKey);

                // 5. 초기화 수행 (진행률: 50~100%)
                await RunInitializationAsync(
                    components.Initializer,
                    config.TargetSceneKey,
                    MapProgress(0.5f, 1f),
                    ct);

                // 6. 이전 Scene 정리
                await UnloadPreviousSceneAsync(targetSceneInstance);

                _previousSceneInstance = targetSceneInstance;
                SceneManager.SetActiveScene(targetSceneInstance.Scene);

                // 7. Loading Scene 언로드
                await UnloadLoadingSceneAsync(config.UseLoadingScene, ct);

                AppLogger.Info($"[SceneLoader] Transition to {config.TargetSceneKey} completed successfully.");

                // 8. 활성화 통지
                NotifyActivated(components.Activatable, config.TargetSceneKey);

                transitionSucceeded = true;
            }
            finally
            {
                // 전환 실패 시 로드된 Scene들 정리
                if (transitionSucceeded == false)
                {
                    await CleanupOnFailureAsync(config.UseLoadingScene, targetSceneInstance);
                }
            }
        }

        /// <summary>
        /// SceneTransitionConfig의 유효성을 검사합니다.
        /// </summary>
        private bool ValidateConfig(SceneTransitionConfig config)
        {
            if (config == null)
            {
                AppLogger.Error("[SceneLoader] SceneTransitionConfig is null.");
                return false;
            }

            if (string.IsNullOrEmpty(config.TargetSceneKey) == true)
            {
                AppLogger.Error($"[SceneLoader] TargetSceneKey is empty in config: {config.name}");
                return false;
            }

            // Loading Scene 유효성 검사
            if (config.UseLoadingScene == true && string.IsNullOrEmpty(config.LoadingSceneKey) == true)
            {
                AppLogger.Error($"[SceneLoader] LoadingSceneKey is empty but UseLoadingScene is true in config: {config.name}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loading Scene을 Additive로 로드합니다.
        /// Loading Scene은 가볍기 때문에 진행률 추적 없이 단순 대기합니다.
        /// </summary>
        private async UniTask<SceneInstance> LoadLoadingSceneAsync(string loadingSceneKey, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(loadingSceneKey) == true)
            {
                throw new ArgumentException("[SceneLoader] Loading scene key is required when useLoadingScene is true.");
            }

            var loadingHandle = Addressables.LoadSceneAsync(loadingSceneKey, LoadSceneMode.Additive);
            await loadingHandle.ToUniTask(cancellationToken: ct);

            if (loadingHandle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new InvalidOperationException($"[SceneLoader] Failed to load loading scene: {loadingSceneKey}");
            }

            // SceneInstance 내부에 핸들 참조가 포함되어 있어 나중에 언로드 가능
            return loadingHandle.Result;
        }

        /// <summary>
        /// 타겟 Scene을 Additive로 로드하면서 진행률을 보고합니다.
        /// </summary>
        private async UniTask<SceneInstance> LoadTargetSceneAsync(
            string sceneKey,
            IProgress<float> progress,
            CancellationToken ct)
        {
            using (LifetimeScope.EnqueueParent(_parentScope))
            {
                var targetHandle = Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Additive, activateOnLoad: true);

                do
                {
                    progress?.Report(targetHandle.PercentComplete);
                    await UniTask.Yield(ct);
                } while (targetHandle.IsDone == false);

                if (targetHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new InvalidOperationException($"[SceneLoader] Failed to load scene: {sceneKey}");
                }

                return targetHandle.Result;
            }
        }

        /// <summary>
        /// 타겟 Scene의 LifetimeScope에서 컴포넌트들을 resolve합니다.
        /// </summary>
        private SceneComponents ResolveSceneComponents(SceneInstance targetSceneInstance)
        {
            var sceneScope = LifetimeScope.Find<LifetimeScope>(targetSceneInstance.Scene);

            ISceneInitializer initializer = null;
            ISceneContext context = null;
            ISceneActivatable activatable = null;

            if (sceneScope != null)
            {
                sceneScope.Container.TryResolve(out initializer);
                sceneScope.Container.TryResolve(out context);
                sceneScope.Container.TryResolve(out activatable);
            }

            return new SceneComponents(initializer, context, activatable);
        }

        /// <summary>
        /// Payload가 존재할 경우 ISceneContext에 전달합니다.
        /// Payload가 있는데 수신할 Context가 없으면 예외를 던집니다.
        /// </summary>
        private void ApplyPayload(ISceneContext context, IScenePayload payload, string sceneKey)
        {
            if (payload == null)
            {
                return;
            }

            if (context == null)
            {
                throw new InvalidOperationException($"[SceneLoader] Payload provided but no ISceneContext found in scene: {sceneKey}");
            }

            context.SetContext(payload);
        }

        /// <summary>
        /// ISceneInitializer가 존재하면 비동기 초기화를 수행하고 진행률을 보고합니다.
        /// 초기화 로직은 Loading Scene이 foreground에 있는 동안 실행됩니다.
        /// </summary>
        private async UniTask RunInitializationAsync(
            ISceneInitializer initializer,
            string sceneKey,
            IProgress<float> progress,
            CancellationToken ct)
        {
            if (initializer == null)
            {
                // 전환 자체는 정상 동작하나, 릴리스 로그 파일에서 "초기화 단계 없이 전환됨"을 추적할 수 있도록 Warning.
                AppLogger.Warning($"[SceneLoader] No ISceneInitializer found in scene: {sceneKey}. Skipping initialization.");
                progress?.Report(1f); // 초기화 없으면 바로 100%
                return;
            }

            await initializer.InitializeAsync(progress, ct);
        }

        /// <summary>
        /// 이전 Scene을 언로드합니다.
        /// 이전 Scene이 없으면(Entry Scene) SceneManager로 현재 active scene을 직접 언로드합니다.
        /// 언로드는 취소되면 안 되는 정리 작업이므로 ct를 전달하지 않습니다.
        /// </summary>
        private async UniTask UnloadPreviousSceneAsync(SceneInstance targetSceneInstance)
        {
            if (_previousSceneInstance.Scene.IsValid() == true)
            {
                await Addressables.UnloadSceneAsync(_previousSceneInstance).ToUniTask();
                return;
            }

            // _previousSceneInstance가 없다는 것은 Entry Scene이므로 SceneManager로 직접 언로드
            var currentActiveScene = SceneManager.GetActiveScene();
            if (currentActiveScene.IsValid() == true && currentActiveScene != targetSceneInstance.Scene)
            {
                await SceneManager.UnloadSceneAsync(currentActiveScene).ToUniTask();
            }
        }

        /// <summary>
        /// Loading Scene을 언로드합니다.
        /// 언로드 직전 한 프레임 양보하여 타겟 Scene이 foreground에 안정적으로 올라오도록 합니다.
        /// </summary>
        private async UniTask UnloadLoadingSceneAsync(bool useLoadingScene, CancellationToken ct)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: ct);

            if (useLoadingScene == true && _loadingSceneInstance.Scene.IsValid() == true)
            {
                await Addressables.UnloadSceneAsync(_loadingSceneInstance).ToUniTask();
            }
        }

        /// <summary>
        /// 전환된 Scene의 ISceneActivatable에 활성화를 통지합니다.
        /// OnActivated에서 예외가 발생하더라도 전환 자체는 성공으로 간주합니다. (예외는 로그 처리)
        /// </summary>
        private void NotifyActivated(ISceneActivatable activatable, string sceneKey)
        {
            if (activatable == null)
            {
                // activatable은 선택적이므로 없다고 해서 전환 실패로 간주하지 않음.
                AppLogger.Debug($"[SceneLoader] No ISceneActivatable in scene: {sceneKey}. Skipping activation notification.");
                return;
            }

            try
            {
                activatable.OnActivated();
            }
            catch (Exception ex)
            {
                AppLogger.Exception(ex, "[SceneLoader] OnActivated threw.");
            }
        }

        /// <summary>
        /// 전환 실패 시 로드된 Scene들을 정리합니다.
        /// 정리 중 예외가 발생하더라도 원래 예외 보존을 위해 catch 처리합니다.
        /// </summary>
        private async UniTask CleanupOnFailureAsync(bool useLoadingScene, SceneInstance targetSceneInstance)
        {
            try
            {
                if (useLoadingScene == true && _loadingSceneInstance.Scene.IsValid() == true)
                {
                    await Addressables.UnloadSceneAsync(_loadingSceneInstance).ToUniTask();
                }
                if (targetSceneInstance.Scene.IsValid() == true)
                {
                    await Addressables.UnloadSceneAsync(targetSceneInstance).ToUniTask();
                }
            }
            catch (Exception ex)
            {
                AppLogger.Warning($"[SceneLoader] Cleanup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 단계 메서드가 보고하는 0~1 진행률을 전체 전환 내 [start, end] 구간으로 매핑하는 IProgress를 생성합니다. 
        /// 각 단계 메서드는 자신의 전체 진행률을 알 필요가 없습니다.
        /// </summary>
        private IProgress<float> MapProgress(float start, float end)
        {
            float range = end - start;
            return new Progress<float>(p =>
            {
                float clamped = Mathf.Clamp01(p);
                _transitionProgress.Value = start + (clamped * range);
            });
        }
        #endregion
    }
}