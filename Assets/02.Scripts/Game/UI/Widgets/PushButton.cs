using CodeJelly.BounceOrBreak.Core.Logging;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Game.UI.Widgets
{
    /// <summary>
    /// 공용 버튼 위젯 클래스.
    /// 클릭을 발행하고, 누르는 동안 눌린 모습을 표현합니다.
    /// </summary>
    public class PushButton : Selectable, IPointerClickHandler
    {
        #region Properties
        public Observable<Unit> OnClick => _onClick;

        /// <summary>
        /// PushButton이 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _face != null;
        #endregion


        #region Private Variables
        [Tooltip("버튼의 면. 누르는 동안 아래로 내려감")]
        [SerializeField] private RectTransform _face;

        private readonly Subject<Unit> _onClick = new Subject<Unit>();

        private Vector2 _restPosition;
        private float _pressDepth;
        private bool _isInitialized;
        #endregion


        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();

            if (IsValid == false)
            {
                AppLogger.Error($"[PushButton] Face is not assigned on {name}.");

                return;
            }

            _restPosition = _face.anchoredPosition;

            // 버튼이 눌리는 깊이의 정본은 프리팹의 면 아래 여백이므로 상수 대신 여기서 읽음
            _pressDepth = _face.offsetMin.y;
            _isInitialized = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _onClick.Dispose();
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// IPointerClickHandler.OnPointerClick Implementation
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // Button과 같이 왼쪽 버튼(터치)만 클릭으로 인정
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (IsActive() == false || IsInteractable() == false)
            {
                return;
            }

            _onClick.OnNext(Unit.Default);
        }
        #endregion


        #region Protected Methods
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            // Awake 전 호출(Edit Mode)이면 무시
            if (_isInitialized == false)
            {
                return;
            }

            _face.anchoredPosition = (state == SelectionState.Pressed)
                                   ? _restPosition + Vector2.down * _pressDepth
                                   : _restPosition;
        }
        #endregion
    }
}
