using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.UI.Message;
using CodeJelly.BounceOrBreak.Game.UI.Navigation;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Remote
{
    /// <summary>
    /// 원격 안내를 메시지 패널로 띄우는 Presenter 클래스.
    /// </summary>
    public class RemoteNoticePresenter
    {
        #region Injection
        private readonly RemoteNoticeService _noticeService;
        private readonly UINavigationService _navigation;

        public RemoteNoticePresenter(
            RemoteNoticeService noticeService,
            UINavigationService navigation)
        {
            _noticeService = noticeService;
            _navigation = navigation;
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// 새 버전 안내와 공지를 차례로 띄웁니다. 없는 것은 건너뜁니다.
        /// </summary>
        public async UniTask ShowPendingAsync(CancellationToken ct)
        {
            await ShowNewVersionAsync(ct);
            await ShowNoticeAsync(ct);
        }
        #endregion


        #region Private Methods
        private async UniTask ShowNewVersionAsync(CancellationToken ct)
        {
            if (_noticeService.TryGetNewerVersion(out string version) == false)
            {
                return;
            }

            bool isAccepted = await _navigation.ShowAsync<MessageRequest, bool>(GameConstants.UI.Keys.MESSAGE, GameConstants.UI.Messages.NewVersion(version), ct);

            // Update는 기록하지 않는다. 업데이트하기 전까지는 다음 실행에도 다시 안내
            if (isAccepted == true)
            {
                AppLogger.Info($"[RemoteNoticePresenter] Opening the store page for v{version}.");
                Application.OpenURL(string.Format(GameConstants.Remote.STORE_URL_FORMAT, Application.identifier));

                return;
            }

            await _noticeService.DismissVersionAsync(version);
        }

        private async UniTask ShowNoticeAsync(CancellationToken ct)
        {
            if (_noticeService.TryGetNotice(DateTime.UtcNow, out RemoteNotice notice) == false)
            {
                return;
            }

            await _navigation.ShowAsync<MessageRequest, bool>(GameConstants.UI.Keys.MESSAGE, GameConstants.UI.Messages.Notice(notice), ct);
            await _noticeService.MarkNoticeSeenAsync(notice.Id);
        }
        #endregion
    }
}