using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Remote;
using CodeJelly.BounceOrBreak.Core.Settings;
using Cysharp.Threading.Tasks;
using System;
using System.Globalization;

namespace CodeJelly.BounceOrBreak.Game.Remote
{
    /// <summary>
    /// 원격 설정을 바탕으로 띄울 안내를 판정하고 열람 기록을 관리하는 클래스.
    /// </summary>
    public class RemoteNoticeService
    {
        #region Injection
        private readonly RemoteConfigService _remoteConfig;
        private readonly ISettingsRepository _repository;
        private readonly AppVersion? _currentVersion;

        /// <param name="currentVersion">이 빌드의 버전 문자열</param>
        public RemoteNoticeService(
            RemoteConfigService remoteConfig,
            ISettingsRepository repository,
            string currentVersion)
        {
            _remoteConfig = remoteConfig;
            _repository = repository;

            if (AppVersion.TryParse(currentVersion, out AppVersion parsed) == true)
            {
                _currentVersion = parsed;
            }
            else
            {
                AppLogger.Warning($"[RemoteNoticeService] Unrecognized app version: {currentVersion}. New version notices are disabled.");
            }
        }
        #endregion


        #region Private Variables
        private RemoteNoticeState _state = new RemoteNoticeState();
        #endregion


        #region Public Methods
        /// <summary>
        /// 저장된 열람 기록을 불러옵니다.
        /// 기록이 없거나 불러오기에 실패하면 기본값으로 시작합니다.
        /// </summary>
        public async UniTask LoadAsync()
        {
            var result = await _repository.LoadAsync<RemoteNoticeState>(GameConstants.Remote.Keys.REPOSITORY_KEY);

            if (result.IsSuccess == true)
            {
                _state = result.Value;
            }
            // NotFound는 최초 실행의 정상 흐름이므로 그 외 실패만 로그
            else if (result.Error != SettingsResultError.NotFound)
            {
                AppLogger.Warning($"[RemoteNoticeService] Failed to load notice state: {result.Error}. Using defaults.");
            }
        }

        /// <summary>
        /// 현재보다 새 버전이 있고 아직 넘기지 않았으면 그 버전을 반환합니다.
        /// </summary>
        public bool TryGetNewerVersion(out string version)
        {
            version = null;

            RemoteConfig config = _remoteConfig.Config;

            if (config == null || _currentVersion.HasValue == false)
            {
                return false;
            }

            if (AppVersion.TryParse(config.LatestVersion, out AppVersion latest) == false)
            {
                return false;
            }

            if (latest.CompareTo(_currentVersion.Value) <= 0)
            {
                return false;
            }

            string normalized = latest.ToString();

            if (normalized == _state.DismissedVersion)
            {
                return false;
            }

            version = normalized;

            return true;
        }

        /// <summary>
        /// 표시 기간 안의 공지 중 아직 보지 않은 것이 있으면 반환합니다.
        /// </summary>
        public bool TryGetNotice(DateTime utcNow, out RemoteNotice notice)
        {
            notice = null;

            RemoteNotice candidate = _remoteConfig.Config?.Notice;

            if (candidate == null || string.IsNullOrEmpty(candidate.Id) == true)
            {
                return false;
            }

            if (candidate.Id == _state.SeenNoticeId)
            {
                return false;
            }

            if (IsWithinPeriod(candidate, utcNow) == false)
            {
                return false;
            }

            notice = candidate;

            return true;
        }

        /// <summary>
        /// 새 버전 안내를 넘긴 것으로 기록합니다.
        /// </summary>
        public UniTask DismissVersionAsync(string version)
        {
            _state.DismissedVersion = version;

            return SaveAsync();
        }

        /// <summary>
        /// 공지를 본 것으로 기록합니다.
        /// </summary>
        public UniTask MarkNoticeSeenAsync(string noticeId)
        {
            _state.SeenNoticeId = noticeId;

            return SaveAsync();
        }
        #endregion


        #region Private Methods
        private async UniTask SaveAsync()
        {
            var result = await _repository.SaveAsync(GameConstants.Remote.Keys.REPOSITORY_KEY, _state);

            if (result.IsSuccess == false)
            {
                AppLogger.Error($"[RemoteNoticeService] Failed to save notice state: {result.Error}");
            }
        }

        private static bool IsWithinPeriod(RemoteNotice notice, DateTime utcNow)
        {
            if (TryParseInstant(notice.From, out DateTime? from) == false || TryParseInstant(notice.Until, out DateTime? until) == false)
            {
                AppLogger.Warning($"[RemoteNoticeService] Notice '{notice.Id}' has an unreadable period. Skipping.");

                return false;
            }

            if (from.HasValue == true && utcNow < from.Value)
            {
                return false;
            }

            if (until.HasValue == true && utcNow >= until.Value)
            {
                return false;
            }

            return true;
        }

        private static bool TryParseInstant(string text, out DateTime? instant)
        {
            instant = null;

            if (string.IsNullOrEmpty(text) == true)
            {
                return true;
            }

            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime parsed) == false)
            {
                return false;
            }

            instant = parsed;

            return true;
        }
        #endregion
    }
}