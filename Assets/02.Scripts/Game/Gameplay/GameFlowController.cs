using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Gameplay.Egg;
using CodeJelly.BounceOrBreak.Game.Gameplay.Goal;
using CodeJelly.BounceOrBreak.Game.Gameplay.Score;
using CodeJelly.BounceOrBreak.Game.Input;
using CodeJelly.BounceOrBreak.Game.Trampoline;
using Cysharp.Threading.Tasks;
using R3;
using System;
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
        private readonly TrampolineDrawer _trampolineDrawer;
        private readonly InputManager _inputManager;

        public GameFlowController(
            FallDetector fallDetector,
            GoalPort[] goalPorts,
            EggSpawner eggSpawner,
            ScoreService scoreService,
            TrampolineDrawer trampolineDrawer,
            InputManager inputManager)
        {
            _fallDetector = fallDetector;
            _goalPorts = goalPorts;
            _eggSpawner = eggSpawner;
            _scoreService = scoreService;
            _trampolineDrawer = trampolineDrawer;
            _inputManager = inputManager;
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
        #endregion


        #region Private Variables
        private readonly ReactiveProperty<GamePhase> _phase = new ReactiveProperty<GamePhase>(GamePhase.Loading);
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
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
                         .Subscribe(_ => TriggerGameOver())
                         .AddTo(_disposables);

            SubscribeGoalPorts();

            _inputManager.SwitchContext(InputContext.Gameplay);

            await _trampolineDrawer.InitializeAsync(null, ct);
            _trampolineDrawer.SetTrampolineType(TrampolineType.Straight);

            await _eggSpawner.InitializeAsync(ct);
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
            _eggSpawner.StartSpawning();
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _disposables.Dispose();
            _phase.Dispose();
        }
        #endregion


        #region Private Methods
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
            }

            // TODO: 불일치 시 재처리
            _eggSpawner.Despawn(arrival.Egg);
        }

        /// <summary>
        /// 게임을 게임오버 상태로 전환합니다.
        /// </summary>
        private void TriggerGameOver()
        {
            if (_phase.Value != GamePhase.Playing)
            {
                return;
            }

            _phase.Value = GamePhase.GameOver;
            _eggSpawner.StopSpawning();

            // 저장 완료를 기다릴 이유가 없으므로 게임오버 처리를 막지 않음
            if (_scoreService.TryUpdateBestScore() == true)
            {
                _scoreService.SaveBestScoreAsync().Forget();
            }
        }
        #endregion
    }
}