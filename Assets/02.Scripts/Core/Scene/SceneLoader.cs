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
            if (config == null)
            {
                AppLogger.Error("[SceneLoader] SceneTransitionConfig is null.");
                return;
            }

            if (string.IsNullOrEmpty(config.TargetSceneKey) == true)
            {
                AppLogger.Error($"[SceneLoader] TargetSceneKey is empty in config: {config.name}");
                return;
            }

            // Loading Scene 유효성 검사
            if (config.UseLoadingScene == true && string.IsNullOrEmpty(config.LoadingSceneKey) == true)
            {
                AppLogger.Error($"[SceneLoader] LoadingSceneKey is empty but UseLoadingScene is true in config: {config.name}");
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
                await TransitionSceneAsync
                (
                    config.TargetSceneKey,
                    payload,
                    config.UseLoadingScene,
                    config.LoadingSceneKey,
                    _transitionCts.Token
                );
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
            string sceneKey,
            IScenePayload payload,
            bool useLoadingScene,
            string loadingSceneKey,
            CancellationToken ct)
        {
            SceneInstance targetSceneInstance = default;
            bool transitionSucceeded = false;

            try
            {
                // 1. Loading Scene이 필요한 경우
                if (useLoadingScene == true)
                {
                    if (string.IsNullOrEmpty(loadingSceneKey) == true)
                    {
                        throw new ArgumentException("[SceneLoader] Loading scene key is required when useLoadingScene is true.");
                    }

                    // Loading Scene은 가볍기 때문에 진행률 추적 없이 단순 대기
                    var loadingHandle = Addressables.LoadSceneAsync(loadingSceneKey, LoadSceneMode.Additive);
                    await loadingHandle.ToUniTask(cancellationToken: ct);

                    if (loadingHandle.Status != AsyncOperationStatus.Succeeded)
                    {
                        throw new InvalidOperationException($"[SceneLoader] Failed to load loading scene: {loadingSceneKey}");
                    }

                    // SceneInstance 내부에 핸들 참조가 포함되어 있어 나중에 언로드 가능
                    // (loadingHandle 로컬 변수는 스코프 종료 시 소멸)
                    _loadingSceneInstance = loadingHandle.Result;
                }

                // 2. 타겟 Scene 로드 (진행률: 0~50%)
                using (LifetimeScope.EnqueueParent(_parentScope))
                {
                    var targetHandle = Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Additive, activateOnLoad: true);

                    do
                    {
                        _transitionProgress.Value = targetHandle.PercentComplete * 0.5f; // 0~50%
                        await UniTask.Yield(ct);
                    } while (targetHandle.IsDone == false);

                    if (targetHandle.Status != AsyncOperationStatus.Succeeded)
                    {
                        throw new InvalidOperationException($"[SceneLoader] Failed to load scene: {sceneKey}");
                    }

                    targetSceneInstance = targetHandle.Result;
                }

                // 3. SceneScope에서 ISceneInitializer, ISceneContext resolve
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

                // 4. Context 설정
                if (payload != null)
                {
                    if (context != null)
                    {
                        context.SetContext(payload);
                    }
                    else
                    {
                        throw new InvalidOperationException($"[SceneLoader] Payload provided but no ISceneContext found in scene: {sceneKey}");
                    }
                }

                // 5. 초기화 수행 (진행률: 50~100%)
                // Loading Scene이 foreground에 있는 동안 실행됨.
                if (initializer != null) // 분할 초기화 프로세스가 필요한 Scene의 경우
                {
                    var progressHandler = new Progress<float>(p =>
                    {
                        _transitionProgress.Value = 0.5f + (p * 0.5f);
                    });

                    await initializer.InitializeAsync(progressHandler, ct);
                }
                else
                {
                    // ISceneInitializer가 없다면 SceneLoader.LoadSceneAsync를 쓸 이유가 적으므로 Warning 로그 처리
                    // (ISceneInitializer가 없어도 전환 자체는 정상 동작)
                    AppLogger.Warning($"[SceneLoader] No ISceneInitializer found in scene: {sceneKey}. Skipping initialization.");
                    _transitionProgress.Value = 1f; // 초기화 없으면 바로 100%
                }

                // 6. 이전 Scene 정리
                if (_previousSceneInstance.Scene.IsValid() == true)
                {
                    // 언로드는 취소되면 안 되는 정리 작업이므로 ct 넘기지 않음
                    await Addressables.UnloadSceneAsync(_previousSceneInstance).ToUniTask();
                }
                else
                {
                    // _previousSceneInstance가 없다는 것은 Entry Scene이므로 SceneManager로 직접 언로드
                    var currentActiveScene = SceneManager.GetActiveScene();
                    if (currentActiveScene.IsValid() == true && currentActiveScene != targetSceneInstance.Scene)
                    {
                        await SceneManager.UnloadSceneAsync(currentActiveScene).ToUniTask();
                    }
                }

                _previousSceneInstance = targetSceneInstance;
                SceneManager.SetActiveScene(targetSceneInstance.Scene);

                // 7. Loading Scene 언로드
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: ct);

                if (useLoadingScene == true && _loadingSceneInstance.Scene.IsValid() == true)
                {
                    await Addressables.UnloadSceneAsync(_loadingSceneInstance).ToUniTask();
                }

                AppLogger.Info($"[SceneLoader] Transition to {sceneKey} completed successfully.");

                // 8. 활성화 통지
                if (activatable != null)
                {
                    // 전환된 Scene의 OnActivated에서 예외가 발생하더라도 전환 자체는 성공으로 간주 (예외는 로그 처리)
                    try
                    {
                        activatable.OnActivated();
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Exception(ex, "[SceneLoader] OnActivated threw.");
                    }
                }

                transitionSucceeded = true;
            }
            finally
            {
                // 전환 실패 시 로드된 Scene들 정리
                if (transitionSucceeded == false)
                {
                    // 정리 중 예외 발생 시 원래 예외의 보존을 위해 catch 처리
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
            }
        }
        #endregion
    }
}