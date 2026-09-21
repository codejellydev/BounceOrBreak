using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.UI.Overlay
{
    /// <summary>
    /// 화면 전체를 덮는 오버레이 클래스.
    /// 투명도를 조절해 전환 연출을 수행합니다.
    /// </summary>
    public class ScreenFader : MonoBehaviour, ITransitionEffect
    {
        #region Properties
        /// <summary>
        /// ScreenFader가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid => _canvas != null && _canvasGroup != null;
        #endregion


        #region Private Variables
        [Tooltip("화면을 덮는 Canvas")]
        [SerializeField] private Canvas _canvas;

        [Tooltip("화면을 덮는 CanvasGroup")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Tooltip("페이드 한 번의 지속 시간 (초)")]
        [SerializeField] private float _duration = 0.1f;

        private CancellationToken _destroyToken;
        #endregion


        #region Unity Lifecycle
        private void Awake()
        {
            if (IsValid == false)
            {
                AppLogger.Error("[ScreenFader] Canvas or CanvasGroup is not assigned.");
                enabled = false;

                return;
            }

            _destroyToken = this.GetCancellationTokenOnDestroy();

            ApplyAlpha(0f);
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// ITransitionEffect.CoverAsync Implementation
        /// </summary>
        public UniTask CoverAsync(CancellationToken cancellationToken = default)
        {
            return FadeAsync(1f, cancellationToken);
        }

        /// <summary>
        /// ITransitionEffect.RevealAsync Implementation
        /// </summary>
        public UniTask RevealAsync(CancellationToken cancellationToken = default)
        {
            return FadeAsync(0f, cancellationToken);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 목표 투명도까지 서서히 변화시킵니다.
        /// </summary>
        private async UniTask FadeAsync(float targetAlpha, CancellationToken cancellationToken)
        {
            if (IsValid == false)
            {
                return;
            }

            // 오브젝트가 파괴되면 연출도 중단
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _destroyToken))
            {
                float startAlpha = _canvasGroup.alpha;
                float elapsed = 0f;

                _canvas.enabled = true;
                _canvasGroup.blocksRaycasts = true;

                try
                {
                    while (elapsed < _duration)
                    {
                        // 전환 연출은 게임 시간과 무관
                        elapsed += Time.unscaledDeltaTime;
                        _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / _duration);

                        await UniTask.Yield(PlayerLoopTiming.Update, linkedCts.Token);
                    }
                }
                finally
                {
                    // 취소·예외로 빠져나와도 목표 값은 적용되어야 함
                    if (_canvasGroup != null)
                    {
                        ApplyAlpha(targetAlpha);
                    }
                }
            }
        }

        /// <summary>
        /// 투명도에 맞춰 입력 차단과 Canvas 활성화를 함께 조정합니다.
        /// </summary>
        private void ApplyAlpha(float alpha)
        {
            bool isCovering = (alpha > 0f);

            _canvasGroup.alpha = alpha;
            _canvasGroup.blocksRaycasts = isCovering;
            _canvas.enabled = isCovering;
        }
        #endregion
    }
}