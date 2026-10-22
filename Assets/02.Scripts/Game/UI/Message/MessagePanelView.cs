using CodeJelly.BounceOrBreak.Game.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeJelly.BounceOrBreak.Game.UI.Message
{
    /// <summary>
    /// 범용 메시지 패널의 View 클래스.
    /// MessagePanelPresenter로부터 제어됩니다.
    /// </summary>
    public class MessagePanelView : MonoBehaviour
    {
        #region Properties
        public PushButton PrimaryButton => _primaryButton;
        public PushButton SecondaryButton => _secondaryButton;

        /// <summary>
        /// 패널 외부(Dim 영역) 클릭으로 패널을 닫는 버튼.
        /// </summary>
        public Button DimBackgroundButton => _dimBackgroundButton;

        /// <summary>
        /// View가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _modal != null
                            && _header != null
                            && _body != null
                            && _footer != null
                            && _titleText != null
                            && _bodyText != null
                            && _primaryButton != null
                            && _primaryFace != null
                            && _primaryLabel != null
                            && _secondaryButton != null
                            && _secondaryLabel != null
                            && _dimBackgroundButton != null;
        #endregion


        #region Private Variables
        [Header("Layout")]
        [SerializeField] private RectTransform _modal;
        [SerializeField] private RectTransform _header;
        [SerializeField] private RectTransform _body;
        [SerializeField] private RectTransform _footer;

        [Header("Texts")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _bodyText;

        [Header("Buttons")]
        [SerializeField] private PushButton _primaryButton;
        [SerializeField] private Image _primaryFace;
        [SerializeField] private TMP_Text _primaryLabel;
        [SerializeField] private PushButton _secondaryButton;
        [SerializeField] private TMP_Text _secondaryLabel;
        [SerializeField] private Button _dimBackgroundButton;

        [Header("Colors")]
        [Tooltip("주 버튼 면 색")]
        [SerializeField] private Color _faceColor = Color.white;
        [Tooltip("주 버튼 라벨 색")]
        [SerializeField] private Color _labelColor = Color.black;
        [Tooltip("경고 상태의 주 버튼 면 색")]
        [SerializeField] private Color _destructiveFaceColor = Color.red;
        [Tooltip("경고 상태의 주 버튼 라벨 색")]
        [SerializeField] private Color _destructiveLabelColor = Color.white;
        #endregion


        #region Public Methods
        /// <summary>
        /// 요청 내용을 표시하고 모달 높이를 내용에 맞춥니다.
        /// </summary>
        public void Show(MessageRequest request)
        {
            // 제목·본문 높이는 글자 수에 따라 달라지므로 TMP가 계산한 값을 정수 도트로 올림
            _titleText.text = request.Title;
            _titleText.ForceMeshUpdate();

            float headerHeight = Mathf.Ceil(_titleText.preferredHeight) + GameConstants.UI.MessagePanel.HEADER_PADDING;

            _header.sizeDelta = new Vector2(_header.sizeDelta.x, headerHeight);

            bool hasBody = string.IsNullOrEmpty(request.Body) == false;
            float bodyHeight = 0f;

            _body.gameObject.SetActive(hasBody);
            _body.anchoredPosition = new Vector2(_body.anchoredPosition.x, -headerHeight);
            _bodyText.text = request.Body;

            if (hasBody == true)
            {
                _bodyText.ForceMeshUpdate();
                bodyHeight = Mathf.Ceil(_bodyText.preferredHeight) + GameConstants.UI.MessagePanel.BODY_PADDING;
                _body.sizeDelta = new Vector2(_body.sizeDelta.x, bodyHeight);
            }

            _primaryLabel.text = request.PrimaryLabel;
            _primaryFace.color = request.IsDestructive ? _destructiveFaceColor : _faceColor;
            _primaryLabel.color = request.IsDestructive ? _destructiveLabelColor : _labelColor;

            bool hasSecondary = string.IsNullOrEmpty(request.SecondaryLabel) == false;

            _secondaryButton.gameObject.SetActive(hasSecondary);
            _secondaryLabel.text = request.SecondaryLabel;

            float footerHeight = _footer.rect.height;
            float footerMargin = _footer.anchoredPosition.y;
            float height = headerHeight + bodyHeight + footerHeight + footerMargin;

            if (hasBody == false)
            {
                height += GameConstants.UI.MessagePanel.EMPTY_BODY_GAP;
            }

            _modal.sizeDelta = new Vector2(_modal.sizeDelta.x, height);
        }
        #endregion
    }
}