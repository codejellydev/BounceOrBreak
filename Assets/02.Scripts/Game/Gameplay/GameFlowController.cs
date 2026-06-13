using CodeJelly.BounceOrBreak.Core.Scene;
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

        public GameFlowController(FallDetector fallDetector)
        {
            _fallDetector = fallDetector;
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
        public UniTask InitializeAsync(IProgress<float> progress, CancellationToken ct)
        {
            InitializationState = SceneInitializationState.Initializing;

            _fallDetector.OnCharacterFell
                         .Subscribe(_ => TriggerGameOver())
                         .AddTo(_disposables);

            progress?.Report(1f);
            InitializationState = SceneInitializationState.Completed;
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// ISceneActivatable.OnActivated Implementation
        /// </summary>
        public void OnActivated()
        {
            _phase.Value = GamePhase.Playing;
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
        /// 게임을 게임오버 상태로 전환합니다.
        /// </summary>
        private void TriggerGameOver()
        {
            if (_phase.Value != GamePhase.Playing)
            {
                return;
            }

            _phase.Value = GamePhase.GameOver;
        }
        #endregion
    }
}