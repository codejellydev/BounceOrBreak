using CodeJelly.BounceOrBreak.Core;
using CodeJelly.BounceOrBreak.Core.Logging;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeJelly.BounceOrBreak.Game.Input
{
    /// <summary>
    /// 게임 전체의 입력 시스템을 관리하는 Singleton Manager.
    /// New Input System을 기반으로 Context별 Input Action Map을 관리합니다.
    /// Event Broadcasting 및 Property를 관리하고 실제 동작은 각 담당 클래스에서 처리.
    /// </summary>
    public class InputManager : Singleton<InputManager>,
        GameInputAction.IPlayerActions,
        GameInputAction.IUIActions,
        GameInputAction.ISystemActions
    {
        #region Events
        // Player Input Events
        /// <summary>
        /// 플레이어 터치가 시작되었을 때 발생하는 이벤트.
        /// </summary>
        public Observable<Vector2> OnPlayerTouchStarted => _onPlayerTouchStarted;

        /// <summary>
        /// 플레이어 터치가 취소되었을 때 발생하는 이벤트.
        /// </summary>
        public Observable<Unit> OnPlayerTouchCanceled => _onPlayerTouchCanceled;

        /// <summary>
        /// 플레이어 드래그가 수행되었을 때 발생하는 이벤트.
        /// Screen Space에서의 현재 터치 위치를 전달합니다.
        /// </summary>
        public Observable<Vector2> OnPlayerDragPerformed => _onPlayerDragPerformed;

        // UI Input Events
        /// <summary>
        /// UI 터치가 시작되었을 때 발생하는 이벤트.
        /// </summary>
        public Observable<Unit> OnUITouchStarted => _onUITouchStarted;

        /// <summary>
        /// UI 터치가 종료되었을 때 발생하는 이벤트.
        /// </summary>
        public Observable<Unit> OnUITouchEnded => _onUITouchEnded;

        // System Input Events
        /// <summary>
        /// 뒤로가기 버튼이 눌렸을 때 발생하는 이벤트.
        /// </summary>
        public Observable<Unit> OnBackPressed => _onBackPressed;
        #endregion


        #region Properties
        /// <summary>
        /// Player Input에서 감지된 터치의 Screen Space 좌표.
        /// </summary>
        public ReadOnlyReactiveProperty<Vector2> PlayerTouchPosition => _playerTouchPosition;

        /// <summary>
        /// UI Input에서 감지된 터치의 Screen Space 좌표.
        /// </summary>
        public ReadOnlyReactiveProperty<Vector2> UITouchPosition => _uiTouchPosition;
        #endregion


        #region Private Variables
        [Header("Input Configuration")]
        [SerializeField] private InputConfig _inputConfig;

        private GameInputAction _gameInputAction;
        private InputContext _currentInputContext = InputContext.None;
        private Dictionary<InputContext, InputConfig.InputProfile> _profileMap;
        private Dictionary<InputMapType, InputActionMap> _actionMapTable;

        // Player Input Subjects
        private readonly Subject<Vector2> _onPlayerTouchStarted = new Subject<Vector2>();
        private readonly Subject<Unit> _onPlayerTouchCanceled = new Subject<Unit>();
        private readonly Subject<Vector2> _onPlayerDragPerformed = new Subject<Vector2>();
        private readonly ReactiveProperty<Vector2> _playerTouchPosition = new ReactiveProperty<Vector2>();

        // UI Input Subjects
        private readonly Subject<Unit> _onUITouchStarted = new Subject<Unit>();
        private readonly Subject<Unit> _onUITouchEnded = new Subject<Unit>();
        private readonly ReactiveProperty<Vector2> _uiTouchPosition = new ReactiveProperty<Vector2>();

        // System Input Subjects
        private readonly Subject<Unit> _onBackPressed = new Subject<Unit>();
        #endregion


        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();

            InitializeInputActions();
            InitializeProfileMap();

            _gameInputAction.Enable();
            BindActionCallbacks();

            SwitchContext(InputContext.Loading);

            AppLogger.Info("[InputManager] Initialized.");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            UnbindActionCallbacks();

            _gameInputAction?.Disable();
            _gameInputAction?.Dispose();

            // Player Input
            _onPlayerTouchStarted?.Dispose();
            _onPlayerTouchCanceled?.Dispose();
            _onPlayerDragPerformed?.Dispose();
            _playerTouchPosition?.Dispose();

            // UI Input
            _onUITouchStarted?.Dispose();
            _onUITouchEnded?.Dispose();
            _uiTouchPosition?.Dispose();

            // System Input
            _onBackPressed?.Dispose();
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// Input Context를 변경하고 해당 Context의 Input Action Map을 활성화합니다.
        /// </summary>
        /// <param name="newContext">전환할 새로운 Input Context</param>
        public void SwitchContext(InputContext newContext)
        {
            if (_currentInputContext == newContext)
            {
                AppLogger.Debug($"[InputManager] Context is already {newContext}. Ignoring.");
                return;
            }

            if (newContext == InputContext.None)
            {
                AppLogger.Warning("[InputManager] Setting InputContext.None. All input maps will be disabled.");
            }

            AppLogger.Debug($"[InputManager] Context changed: {_currentInputContext} -> {newContext}");

            _currentInputContext = newContext;
            ApplyContextSettings(newContext);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// Input Action을 초기화합니다.
        /// </summary>
        private void InitializeInputActions()
        {
            _gameInputAction = new GameInputAction();
            InitializeActionMapTable();
        }

        /// <summary>
        /// InputMapType과 InputActionMap의 매핑 테이블을 초기화합니다.
        /// </summary>
        private void InitializeActionMapTable()
        {
            _actionMapTable = new Dictionary<InputMapType, InputActionMap>
            {
                { InputMapType.Player, _gameInputAction.Player },
                { InputMapType.UI, _gameInputAction.UI },
                { InputMapType.System, _gameInputAction.System },
            };
        }

        /// <summary>
        /// InputConfig의 Profile 배열을 Dictionary로 변환하여 캐싱합니다.
        /// </summary>
        private void InitializeProfileMap()
        {
            if (_inputConfig == null)
            {
                AppLogger.Error("[InputManager] InputConfig is not assigned.");
                return;
            }

            _profileMap = new Dictionary<InputContext, InputConfig.InputProfile>();

            foreach (var profile in _inputConfig.Profiles)
            {
                _profileMap[profile.Context] = profile;
            }
        }

        /// <summary>
        /// 지정된 Context에 맞는 Input Action Map 설정을 적용합니다.
        /// </summary>
        /// <param name="context">적용할 Input Context</param>
        private void ApplyContextSettings(InputContext context)
        {
            if (_profileMap.TryGetValue(context, out var profile) == false)
            {
                AppLogger.Error($"[InputManager] Profile not found for context: {context}");
                return;
            }

            SetInputMaps(profile.ActiveMapFlags);
        }

        /// <summary>
        /// 지정된 Input Map Flags에 따라 Action Map을 활성화/비활성화합니다.
        /// </summary>
        /// <param name="enabledMaps">활성화할 Input Map Flags</param>
        private void SetInputMaps(InputMapType enabledMaps)
        {
            foreach (var kvp in _actionMapTable)
            {
                if (enabledMaps.HasFlag(kvp.Key))
                {
                    kvp.Value.Enable();
                }
                else
                {
                    kvp.Value.Disable();
                }
            }
        }

        /// <summary>
        /// Input Action의 콜백 인터페이스를 등록합니다.
        /// </summary>
        private void BindActionCallbacks()
        {
            if (_gameInputAction == null)
            {
                return;
            }

            _gameInputAction.Player.SetCallbacks(this);
            _gameInputAction.UI.SetCallbacks(this);
            _gameInputAction.System.SetCallbacks(this);

            AppLogger.Debug("[InputManager] Input callbacks registered.");
        }

        /// <summary>
        /// Input Action의 콜백 인터페이스 등록을 해제합니다.
        /// </summary>
        private void UnbindActionCallbacks()
        {
            if (_gameInputAction == null)
            {
                return;
            }

            _gameInputAction.Player.SetCallbacks(null);
            _gameInputAction.UI.SetCallbacks(null);
            _gameInputAction.System.SetCallbacks(null);

            AppLogger.Debug("[InputManager] Input callbacks unregistered.");
        }
        #endregion


        #region Input Action Callbacks
        void GameInputAction.IPlayerActions.OnTouch(InputAction.CallbackContext context)
        {
            if (context.started == true)
            {
                Vector2 position = _gameInputAction.Player.Position.ReadValue<Vector2>();
                _playerTouchPosition.Value = position;
                _onPlayerTouchStarted.OnNext(position);
            }
            else if (context.canceled == true)
            {
                _onPlayerTouchCanceled.OnNext(Unit.Default);
            }
        }

        void GameInputAction.IPlayerActions.OnDrag(InputAction.CallbackContext context)
        {
            if (context.performed == true)
            {
                Vector2 position = _gameInputAction.Player.Position.ReadValue<Vector2>();
                _playerTouchPosition.Value = position;
                _onPlayerDragPerformed.OnNext(position);
            }
        }

        void GameInputAction.IPlayerActions.OnPosition(InputAction.CallbackContext context)
        {
            // IPlayerActions.OnTouch, IPlayerActions.OnDrag에서 _playerTouchPosition를 업데이트하기 때문에 중복 업데이트 방지를 위해 비움.
            // _gameInputAction.Player.Position 접근을 위해 InputAction asset에는 존재 필요.
        }

        void GameInputAction.IUIActions.OnTouch(InputAction.CallbackContext context)
        {
            if (context.started == true)
            {
                Vector2 position = _gameInputAction.UI.Position.ReadValue<Vector2>();
                _uiTouchPosition.Value = position;
                _onUITouchStarted.OnNext(Unit.Default);
            }
            else if (context.canceled == true)
            {
                _onUITouchEnded.OnNext(Unit.Default);
            }
        }

        void GameInputAction.IUIActions.OnPosition(InputAction.CallbackContext context)
        {
            // IUIActions.OnTouch에서 _playerTouchPosition를 업데이트하기 때문에 중복 업데이트 방지를 위해 비움.
            // _gameInputAction.UI.Position 접근을 위해 InputAction asset에는 존재 필요.
        }

        void GameInputAction.ISystemActions.OnBack(InputAction.CallbackContext context)
        {
            if (context.started == true)
            {
                _onBackPressed.OnNext(Unit.Default);
            }
        }
        #endregion
    }
}
