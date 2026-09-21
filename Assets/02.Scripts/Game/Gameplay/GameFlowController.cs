using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Pause;
using CodeJelly.BounceOrBreak.Core.Resource;
using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Character;
using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using CodeJelly.BounceOrBreak.Game.Gameplay.Goal;
using CodeJelly.BounceOrBreak.Game.Gameplay.Score;
using CodeJelly.BounceOrBreak.Game.Gameplay.Stage;
using CodeJelly.BounceOrBreak.Game.Input;
using CodeJelly.BounceOrBreak.Game.Trampoline;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CodeJelly.BounceOrBreak.Game.Gameplay
{
    /// <summary>
    /// GameScene의 생명주기와 게임 플로우를 제어하는 클래스.
    /// </summary>
    public class GameFlowController : ISceneInitializer, ISceneActivatable, IDisposable
    {
        #region Injection
        private readonly FallDetector _fallDetector;
        private readonly GoalPort[] _goalPorts;
        private readonly EggSpawner _eggSpawner;
        private readonly ScoreService _scoreService;
        private readonly RoundProgress _roundProgress;
        private readonly TrampolineDrawer _trampolineDrawer;
        private readonly InputManager _inputManager;
        private readonly ResourceManager _resourceManager;

        public GameFlowController(
            FallDetector fallDetector,
            GoalPort[] goalPorts,
            EggSpawner eggSpawner,
            ScoreService scoreService,
            RoundProgress roundProgress,
            TrampolineDrawer trampolineDrawer,
            InputManager inputManager,
            ResourceManager resourceManager)
        {
            _fallDetector = fallDetector;
            _goalPorts = goalPorts;
            _eggSpawner = eggSpawner;
            _scoreService = scoreService;
            _roundProgress = roundProgress;
            _trampolineDrawer = trampolineDrawer;
            _inputManager = inputManager;
            _resourceManager = resourceManager;

            // 정지 대상은 여기 한 곳에만 나열. 시간이 흐르는 시스템이 생기면 여기에 추가
            _pauseables.Add(eggSpawner);
            _pauseables.Add(trampolineDrawer);

            if (goalPorts != null)
            {
                _pauseables.AddRange(goalPorts);
            }
        }
        #endregion


        #region Properties
        /// <summary>
        /// ISceneInitializer.InitializationState Implementation
        /// </summary>
        public SceneInitializationState InitializationState { get; private set; } = SceneInitializationState.None;

        /// <summary>
        /// 현재 게임 진행 단계.
        /// </summary>
        public ReadOnlyReactiveProperty<GamePhase> Phase => _phase.ToReadOnlyReactiveProperty();

        /// <summary>
        /// 게임이 멈춰 있는지 여부. Phase와 직교합니다.
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsPaused => _isPaused.ToReadOnlyReactiveProperty();

        /// <summary>
        /// Round가 끝났을 때 발행
        /// </summary>
        public Observable<RoundResult> OnRoundEnded => _onRoundEnded;
        #endregion


        #region Private Variables
        private readonly ReactiveProperty<GamePhase> _phase = new ReactiveProperty<GamePhase>(GamePhase.Loading);
        private readonly ReactiveProperty<bool> _isPaused = new ReactiveProperty<bool>(false);
        private readonly Subject<RoundResult> _onRoundEnded = new Subject<RoundResult>();
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly List<IPauseable> _pauseables = new List<IPauseable>();

        private StageConfig _stageConfig;
        private CancellationTokenSource _roundCts;
        #endregion


        #region Public Methods
        /// <summary>
        /// ISceneInitializer.InitializeAsync Implementation
        /// </summary>
        public async UniTask InitializeAsync(IProgress<float> progress, CancellationToken ct)
        {
            if (InitializationState != SceneInitializationState.None)
            {
                AppLogger.Debug($"[GameFlowController] State is {InitializationState}. Skipping.");

                return;
            }

            InitializationState = SceneInitializationState.Initializing;

            _fallDetector.OnCharacterFell
                         .Subscribe(TriggerGameOver)
                         .AddTo(_disposables);

            SubscribeGoalPorts();

            _roundProgress.SpawnedCount
                          .Subscribe(UpdateGoalPortStates)
                          .AddTo(_disposables);

            _inputManager.SwitchContext(InputContext.Gameplay);

            await _trampolineDrawer.InitializeAsync(null, ct);
            _trampolineDrawer.SetTrampolineType(TrampolineType.Straight);

            _stageConfig = await _resourceManager.LoadAsync<StageConfig>(GameConstants.Stage.Keys.DEFAULT_CONFIG, ct);

            EggSpawnConfig spawnConfig = (_stageConfig != null) ? _stageConfig.SpawnConfig : null;

            await _eggSpawner.InitializeAsync(spawnConfig, ct);
            ValidateGoalPortColors();

            SpawnStageTrampolines();

            await _scoreService.InitializeAsync();
            progress?.Report(1f);

            InitializationState = SceneInitializationState.Completed;
        }

        /// <summary>
        /// ISceneActivatable.OnActivated Implementation
        /// </summary>
        public void OnActivated()
        {
            _phase.Value = GamePhase.Playing;
            StartRound();
        }

        /// <summary>
        /// GameOver 상태의 게임을 정리하고 재시작합니다.
        /// </summary>
        public void RestartRound()
        {
            if (_phase.Value != GamePhase.GameOver)
            {
                AppLogger.Warning($"[GameFlowController] Restart requested in {_phase.Value} phase. Ignored.");

                return;
            }

            if (_isPaused.Value == true)
            {
                AppLogger.Warning("[GameFlowController] Restart requested while paused. Ignored.");

                return;
            }

            AppLogger.Info("[GameFlowController] Restarting.");

            // Playing으로 전환되기 전에 리셋해야 리셋 중 발생하는 Arrival 이벤트가 득점으로 처리되지 않음
            _trampolineDrawer.DespawnAll();
            _scoreService.ResetScore();
            SpawnStageTrampolines();

            _phase.Value = GamePhase.Playing;
            _inputManager.SwitchContext(InputContext.Gameplay);

            StartRound();
        }

        /// <summary>
        /// 게임을 멈춥니다. 로딩 중이거나 이미 멈춰 있으면 무시합니다.
        /// </summary>
        public void Pause()
        {
            if (_phase.Value == GamePhase.Loading)
            {
                AppLogger.Warning("[GameFlowController] Pause requested in Loading phase. Ignored.");

                return;
            }

            if (_isPaused.Value == true)
            {
                return;
            }

            AppLogger.Info("[GameFlowController] Paused.");

            // 컨텍스트 전환 시 입력 취소 콜백이 Trampoline을 만들므로 드로잉 취소를 먼저 수행
            foreach (IPauseable pauseable in _pauseables)
            {
                pauseable.Pause();
            }

            _inputManager.SwitchContext(InputContext.Pause);
            _isPaused.Value = true;
        }

        /// <summary>
        /// 멈춘 게임을 이어갑니다. 멈춰 있지 않으면 무시합니다.
        /// </summary>
        public void Resume()
        {
            if (_isPaused.Value == false)
            {
                return;
            }

            AppLogger.Info("[GameFlowController] Resumed.");

            foreach (IPauseable pauseable in _pauseables)
            {
                pauseable.Resume();
            }

            _inputManager.SwitchContext(GetInputContext(_phase.Value));
            _isPaused.Value = false;
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            CancelRound();

            _disposables.Dispose();
            _phase.Dispose();
            _isPaused.Dispose();
            _onRoundEnded.Dispose();

            if (_stageConfig != null)
            {
                _resourceManager.Release(GameConstants.Stage.Keys.DEFAULT_CONFIG, ResourceReleaseContext.Cleanup);
            }
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Phase에 맞는 InputContext를 반환합니다.
        /// </summary>
        private static InputContext GetInputContext(GamePhase phase)
        {
            return (phase == GamePhase.GameOver) ? InputContext.GameOver : InputContext.Gameplay;
        }

        /// <summary>
        /// 낙하한 ICharacter의 Egg 데이터를 반환합니다.
        /// </summary>
        /// <returns>Egg 데이터. Egg가 아니면 null</returns>
        private static EggData GetBrokenEgg(ICharacter fallen)
        {
            // FallDetector는 캐릭터 층이라 Egg를 모르므로 같은 오브젝트의 IEgg를 직접 찾음
            return fallen.Transform.TryGetComponent(out IEgg egg) ? egg.Data : null;
        }

        /// <summary>
        /// 모든 GoalPort의 도달 이벤트를 구독합니다.
        /// </summary>
        private void SubscribeGoalPorts()
        {
            if (_goalPorts == null || _goalPorts.Length == 0)
            {
                AppLogger.Error("[GameFlowController] No GoalPort is registered.");

                return;
            }

            foreach (GoalPort port in _goalPorts)
            {
                port.OnEggArrived
                    .Subscribe(HandleGoalArrival)
                    .AddTo(_disposables);
            }
        }

        /// <summary>
        /// 각 GoalPort가 요구하는 EggColor가 실제로 스폰되는지 검증합니다.
        /// </summary>
        private void ValidateGoalPortColors()
        {
            if (_goalPorts == null || _goalPorts.Length == 0)
            {
                AppLogger.Error("[GameFlowController] No GoalPort to validate.");

                return;
            }

            foreach (GoalPort port in _goalPorts)
            {
                if (_eggSpawner.CanSpawnColor(port.TargetColor) == false)
                {
                    AppLogger.Error($"[GameFlowController] {port.name} requires {port.TargetColor}, which no wave spawns.");
                }
            }
        }

        /// <summary>
        /// 누적 스폰 횟수에 따라 각 GoalPort의 개방 여부를 갱신합니다.
        /// </summary>
        private void UpdateGoalPortStates(int spawnedCount)
        {
            if (_goalPorts == null)
            {
                return;
            }

            foreach (GoalPort port in _goalPorts)
            {
                port.SetOpen(port.RequiredSpawnCount <= spawnedCount);
            }
        }

        /// <summary>
        /// 새 게임을 시작합니다.
        /// </summary>
        private void StartRound()
        {
            _roundCts = new CancellationTokenSource();

            // StartNewRound가 활성 Egg 회수와 진행 초기화를 수행함
            _eggSpawner.StartNewRound();
        }

        /// <summary>
        /// StageConfig에 정의된 Trampoline을 스폰합니다.
        /// </summary>
        private void SpawnStageTrampolines()
        {
            if (_stageConfig == null)
            {
                AppLogger.Error("[GameFlowController] StageConfig is not loaded. Skipping stage trampolines.");

                return;
            }

            foreach (TrampolineSpawnData spawnData in _stageConfig.Trampolines)
            {
                _trampolineDrawer.Spawn(spawnData);
            }
        }

        /// <summary>
        /// 진행 중인 Round의 비동기 작업을 모두 취소합니다.
        /// </summary>
        private void CancelRound()
        {
            _roundCts?.Cancel();
            _roundCts?.Dispose();
            _roundCts = null;
        }

        /// <summary>
        /// 게임을 게임오버 상태로 전환합니다.
        /// </summary>
        private void TriggerGameOver(ICharacter fallen)
        {
            if (_phase.Value != GamePhase.Playing)
            {
                return;
            }

            _phase.Value = GamePhase.GameOver;
            AppLogger.Info($"[GameFlowController] Game over. Score: {_scoreService.CurrentScore.CurrentValue}");

            _inputManager.SwitchContext(InputContext.GameOver);
            CancelRound();
            _eggSpawner.StopSpawning();

            bool isNewBest = _scoreService.TryUpdateBestScore();

            // 저장 완료를 기다릴 이유가 없으므로 게임오버 처리를 막지 않음
            if (isNewBest == true)
            {
                _scoreService.SaveBestScoreAsync().Forget();
            }

            // 값을 스냅샷으로 넘겨 구독자가 RestartRound의 ResetScore에 영향받지 않게 함
            _onRoundEnded.OnNext(new RoundResult
            {
                Score = _scoreService.CurrentScore.CurrentValue,
                BestScore = _scoreService.BestScore.CurrentValue,
                IsNewBest = isNewBest,
                BrokenEgg = GetBrokenEgg(fallen)
            });
        }

        /// <summary>
        /// Egg의 GoalPort 도달을 처리합니다.
        /// </summary>
        private void HandleGoalArrival(GoalArrival arrival)
        {
            if (_phase.Value != GamePhase.Playing)
            {
                return;
            }

            if (arrival.IsMatched == true)
            {
                _scoreService.AddScore();
                _eggSpawner.Despawn(arrival.Egg);

                return;
            }

            HandleMismatch(arrival);
        }

        /// <summary>
        /// TargetColor와 불일치한 Egg를 스테이지의 MismatchPolicy에 따라 처리합니다.
        /// </summary>
        private void HandleMismatch(GoalArrival arrival)
        {
            // MismatchPolicy을 알 수 없으므로 처리하지 않음 (로드 실패는 StartRound에서 보고)
            if (_stageConfig == null)
            {
                return;
            }

            switch (_stageConfig.MismatchPolicy)
            {
                case MismatchPolicy.Eject:
                    arrival.Port.RejectAsync(arrival.Egg, _roundCts.Token).Forget();
                    break;

                case MismatchPolicy.Absorb:
                    // TODO: 감점 정책 도입 시 여기서 반영
                    _eggSpawner.Despawn(arrival.Egg);
                    break;
            }
        }
        #endregion
    }
}
