using CodeJelly.BounceOrBreak.Core.Logging;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace CodeJelly.BounceOrBreak.Core.Scene
{
    /// <summary>
    /// Addressables 기반 씬 로딩 클래스.
    /// SceneManager와 혼용을 권장하지 않습니다.
    /// (권장 구조: Unity Build Settings에는 Entry 씬만 포함하고, 나머지는 모두 Addressables로 관리)
    /// TODO: 씬 키 enum 자동 생성 및 빌드 프로세스에서의 키 검증 도입
    /// </summary>
    public class SceneLoader : IDisposable
    {
        public SceneLoader()
        {
            _transitionProgress = new ReactiveProperty<float>(0f);
            IsTransitioning = false;
        }


        #region Properties
        /// <summary>
        /// 현재 전환 진행률 (0.0 ~ 1.0)
        /// UI 바인딩에 주로 쓰일 수 있으므로 ReactiveProperty로 선언.
        /// </summary>
        public ReadOnlyReactiveProperty<float> TransitionProgress =>
            _transitionProgress.ToReadOnlyReactiveProperty();

        /// <summary>
        /// 현재 씬 전환 중인지 여부
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
        /// Build Settings에 포함된 씬(Entry 등)에서 Addressables 씬으로 전환할 때 사용합니다.
        /// 일반적인 경우 Entry 씬의 Bootstrap 모듈에서 LoadSceneAsync를 직접 호출하는 것을 권장합니다.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            _previousSceneInstance = default;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// 씬을 비동기적으로 전환합니다.
        /// 초기화 로직, 로딩 씬, Payload 전달 등이 필요한 경우 사용합니다.
        /// </summary>
        /// <param name="config">씬 전환 설정</param>
        /// <param name="payload">씬에 전달할 데이터</param>
        /// <param name="ct">취소 토큰</param>
        public async UniTask LoadSceneAsync(
            SceneTransitionConfig config,
            IPayload payload = null,
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

        // IDisposable Implementation
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
        /// 실제 씬 전환 로직을 실행합니다.
        /// </summary>
        private async UniTask TransitionSceneAsync(
            string sceneKey,
            IPayload payload,
            bool useLoadingScene,
            string loadingSceneKey,
            CancellationToken ct)
        {
            SceneInstance targetSceneInstance = default;
            bool transitionSucceeded = false;

            try
            {
                // 1. 로딩 씬이 필요한 경우
                if (useLoadingScene == true)
                {
                    if (string.IsNullOrEmpty(loadingSceneKey) == true)
                    {
                        throw new ArgumentException("[SceneLoader] Loading scene key is required when useLoadingScene is true.");
                    }

                    // 로딩 씬은 가볍기 때문에 진행률 추적 없이 단순 대기
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

                // 2. 타겟 씬 로드 (진행률: 0~50%)
                var targetHandle = Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Additive, activateOnLoad: false);

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

                // 3. 비활성 씬에서 컴포넌트 탐색
                var loadedTargetScene = targetSceneInstance.Scene;
                if (loadedTargetScene.IsValid() == false)
                {
                    throw new InvalidOperationException($"[SceneLoader] Loaded scene is invalid: {sceneKey}");
                }

                var rootObjects = loadedTargetScene.GetRootGameObjects();

                var initializer = rootObjects
                    .SelectMany(go => go.GetComponentsInChildren<ISceneInitializer>(includeInactive: true))
                    .FirstOrDefault(); // 첫 번째 발견 시 즉시 반환, 없으면 null

                // 4. Context 설정
                if (payload != null)
                {
                    var context = rootObjects
                        .SelectMany(go => go.GetComponentsInChildren<ISceneContext>(includeInactive: true))
                        .FirstOrDefault(); // 첫 번째 발견 시 즉시 반환, 없으면 null

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
                if (initializer != null)
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
                    AppLogger.Warning($"[SceneLoader] No ISceneInitializer found in scene: {sceneKey}. Skipping initialization.");
                    _transitionProgress.Value = 1f; // 초기화 없으면 바로 100%
                }

                // 6. 타겟 씬 활성화
                await targetSceneInstance.ActivateAsync().ToUniTask(cancellationToken: ct);

                // 7. 활성화 직후 한 프레임 대기 (프레임 드랍 완화 안정 장치)
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: ct);

                // 8. 이전 씬 정리
                if (_previousSceneInstance.Scene.IsValid() == true)
                {
                    await Addressables.UnloadSceneAsync(_previousSceneInstance).ToUniTask(default);
                }
                else
                {
                    // _previousSceneInstance가 없다는 것은 Entry 씬이므로 SceneManager로 직접 언로드
                    var currentActiveScene = SceneManager.GetActiveScene();
                    if (currentActiveScene.IsValid() && currentActiveScene != loadedTargetScene)
                    {
                        await SceneManager.UnloadSceneAsync(currentActiveScene).ToUniTask(default);
                    }
                }

                _previousSceneInstance = targetSceneInstance;
                SceneManager.SetActiveScene(loadedTargetScene);

                // 9. 로딩 씬 언로드
                if (useLoadingScene == true && _loadingSceneInstance.Scene.IsValid() == true)
                {
                    await Addressables.UnloadSceneAsync(_loadingSceneInstance).ToUniTask(default);
                }

                AppLogger.Info($"[SceneLoader] Transition to {sceneKey} completed successfully.");

                transitionSucceeded = true;
            }
            finally
            {
                // 전환 실패 시 로드된 씬들 정리
                if (transitionSucceeded == false)
                {
                    // 정리 중 예외 발생 시 원래 예외의 보존을 위해 catch 처리
                    try
                    {
                        if (useLoadingScene == true && _loadingSceneInstance.Scene.IsValid() == true)
                        {
                            await Addressables.UnloadSceneAsync(_loadingSceneInstance).ToUniTask(default);
                        }
                        if (targetSceneInstance.Scene.IsValid() == true)
                        {
                            await Addressables.UnloadSceneAsync(targetSceneInstance).ToUniTask(default);
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