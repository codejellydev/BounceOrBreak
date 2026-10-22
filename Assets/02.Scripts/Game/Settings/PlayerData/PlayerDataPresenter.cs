using CodeJelly.BounceOrBreak.Game.Score;
using CodeJelly.BounceOrBreak.Game.UI.Message;
using CodeJelly.BounceOrBreak.Game.UI.Navigation;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;

namespace CodeJelly.BounceOrBreak.Game.Settings.PlayerData
{
    /// <summary>
    /// 플레이어 데이터 카테고리의 Presenter 클래스.
    /// </summary>
    public class PlayerDataPresenter
    {
        #region Injection
        private readonly ScoreRecordService _scoreRecordService;
        private readonly UINavigationService _navigation;

        public PlayerDataPresenter(
            ScoreRecordService scoreRecordService,
            UINavigationService navigation)
        {
            _scoreRecordService = scoreRecordService;
            _navigation = navigation;
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// View를 데이터 삭제에 연결합니다.
        /// </summary>
        /// <param name="ct">패널이 열려 있는 동안의 토큰</param>
        public void Bind(PlayerDataView view, CompositeDisposable disposables, CancellationToken ct)
        {
            view.ClearRecordButton
                .OnClick
                .Subscribe(_ => ClearRecordAsync(ct).Forget())
                .AddTo(disposables);
        }
        #endregion


        #region Private Methods
        private async UniTask ClearRecordAsync(CancellationToken ct)
        {
            bool isConfirmed;

            try
            {
                isConfirmed = await _navigation.ShowAsync<MessageRequest, bool>(GameConstants.UI.Keys.MESSAGE, GameConstants.UI.Messages.CLEAR_RECORD, ct);
            }
            catch (OperationCanceledException)
            {
                // 씬이 먼저 내려간 것이므로 처리할 결과가 없음
                return;
            }

            if (isConfirmed == false)
            {
                return;
            }

            await _scoreRecordService.ClearAsync();
        }
        #endregion
    }
}