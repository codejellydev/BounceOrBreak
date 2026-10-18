using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.Remote;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// RemoteNoticeService의 안내 판정과 열람 기록 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class RemoteNoticeServiceTests
    {
        #region Private Variables
        private const string CURRENT_VERSION = "0.1.3";
        private const string NOTICE_ID = "2026-10-launch";

        private static readonly DateTime NOW = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

        private FakeRemoteConfigSource _source;
        private FakeSettingsRepository _repository;
        #endregion


        #region Public Methods
        [SetUp]
        public void SetUp()
        {
            _source = new FakeRemoteConfigSource();
            _repository = new FakeSettingsRepository();
        }

        /// <summary>
        /// 최신 버전이 현재 버전보다 높으면 그 버전을 반환합니다.
        /// </summary>
        [Test]
        public async Task TryGetNewerVersion_LatestIsNewer_ReturnsIt()
        {
            RemoteNoticeService service = await CreateAsync(Json(latestVersion: "0.1.4"));

            bool found = service.TryGetNewerVersion(out string version);

            Assert.IsTrue(found);
            Assert.AreEqual("0.1.4", version);
        }

        /// <summary>
        /// 접미사를 뗀 최신 버전이 현재 버전과 같거나 낮으면 안내하지 않습니다.
        /// </summary>
        [TestCase("0.1.4-dev.5+abc1234", "0.1.4")]
        [TestCase("0.1.4", "0.1.4")]
        [TestCase("0.1.5", "0.1.4")]
        public async Task TryGetNewerVersion_LatestNotNewer_ReturnsFalse(string currentVersion, string latestVersion)
        {
            RemoteNoticeService service = await CreateAsync(Json(latestVersion: latestVersion), currentVersion);

            bool found = service.TryGetNewerVersion(out _);

            Assert.IsFalse(found);
        }

        /// <summary>
        /// 설정을 수신하지 못했으면 안내하지 않습니다.
        /// </summary>
        [Test]
        public async Task TryGetNewerVersion_NoConfig_ReturnsFalse()
        {
            _source.Error = Core.Remote.RemoteResultError.NetworkError;
            RemoteNoticeService service = await CreateAsync(null);

            bool found = service.TryGetNewerVersion(out _);

            Assert.IsFalse(found);
        }

        /// <summary>
        /// 최신 버전이나 현재 버전을 해석할 수 없으면 안내하지 않습니다.
        /// </summary>
        [TestCase("0.1.3", "latest")]
        [TestCase("unknown", "0.1.4")]
        public async Task TryGetNewerVersion_UnparsableVersion_ReturnsFalse(string currentVersion, string latestVersion)
        {
            RemoteNoticeService service = await CreateAsync(Json(latestVersion: latestVersion), currentVersion);

            bool found = service.TryGetNewerVersion(out _);

            Assert.IsFalse(found);
        }

        /// <summary>
        /// 넘긴 버전은 다시 안내하지 않고, 더 새 버전은 안내합니다.
        /// </summary>
        [Test]
        public async Task TryGetNewerVersion_Dismissed_SkipsOnlyThatVersion()
        {
            RemoteNoticeService service = await CreateAsync(Json(latestVersion: "0.1.4"));

            await service.DismissVersionAsync("0.1.4");

            Assert.IsFalse(service.TryGetNewerVersion(out _));

            RemoteNoticeService later = await CreateAsync(Json(latestVersion: "0.1.5"));

            Assert.IsTrue(later.TryGetNewerVersion(out string version));
            Assert.AreEqual("0.1.5", version);
        }

        /// <summary>
        /// 넘긴 기록은 저장소에 저장되어 새 인스턴스에서도 유지됩니다.
        /// </summary>
        [Test]
        public async Task DismissVersionAsync_PersistsAcrossInstances()
        {
            RemoteNoticeService service = await CreateAsync(Json(latestVersion: "0.1.4"));

            await service.DismissVersionAsync("0.1.4");

            RemoteNoticeService reloaded = await CreateAsync(Json(latestVersion: "0.1.4"));

            Assert.IsFalse(reloaded.TryGetNewerVersion(out _));
        }

        /// <summary>
        /// 기간 안의 공지를 반환합니다. 비어 있는 기간은 제한이 없는 것으로 취급합니다.
        /// </summary>
        [TestCase("2026-10-01", "2026-10-31")]
        [TestCase("", "")]
        [TestCase("2026-10-15T11:59:59Z", "")]
        [TestCase("", "2026-10-15T12:00:01Z")]
        public async Task TryGetNotice_WithinPeriod_ReturnsNotice(string from, string until)
        {
            RemoteNoticeService service = await CreateAsync(Json(noticeId: NOTICE_ID, from: from, until: until));

            bool found = service.TryGetNotice(NOW, out RemoteNotice notice);

            Assert.IsTrue(found);
            Assert.AreEqual(NOTICE_ID, notice.Id);
        }

        /// <summary>
        /// 시작 전이거나 종료 시각 이후면 반환하지 않습니다.
        /// </summary>
        [TestCase("2026-10-16", "")]
        [TestCase("", "2026-10-15")]
        [TestCase("", "2026-10-15T12:00:00Z")]
        public async Task TryGetNotice_OutsidePeriod_ReturnsFalse(string from, string until)
        {
            RemoteNoticeService service = await CreateAsync(Json(noticeId: NOTICE_ID, from: from, until: until));

            bool found = service.TryGetNotice(NOW, out _);

            Assert.IsFalse(found);
        }

        /// <summary>
        /// 기간을 해석할 수 없는 공지는 반환하지 않습니다.
        /// </summary>
        [Test]
        public async Task TryGetNotice_UnreadablePeriod_ReturnsFalse()
        {
            RemoteNoticeService service = await CreateAsync(Json(noticeId: NOTICE_ID, from: "next week"));

            bool found = service.TryGetNotice(NOW, out _);

            Assert.IsFalse(found);
        }

        /// <summary>
        /// 공지 항목이 없거나 식별자가 비어 있으면 반환하지 않습니다.
        /// </summary>
        [Test]
        public async Task TryGetNotice_NoNotice_ReturnsFalse()
        {
            RemoteNoticeService service = await CreateAsync(Json(noticeId: ""));

            bool found = service.TryGetNotice(NOW, out _);

            Assert.IsFalse(found);
        }

        /// <summary>
        /// 본 공지는 다시 반환하지 않지만 식별자가 다른 공지는 반환합니다.
        /// </summary>
        [Test]
        public async Task TryGetNotice_Seen_SkipsOnlyThatNotice()
        {
            RemoteNoticeService service = await CreateAsync(Json(noticeId: NOTICE_ID));

            await service.MarkNoticeSeenAsync(NOTICE_ID);

            Assert.IsFalse(service.TryGetNotice(NOW, out _));

            RemoteNoticeService later = await CreateAsync(Json(noticeId: "other"));

            Assert.IsTrue(later.TryGetNotice(NOW, out RemoteNotice notice));
            Assert.AreEqual("other", notice.Id);
        }

        /// <summary>
        /// 열람 기록은 저장소에 저장되어 새 인스턴스에서도 유지됩니다.
        /// </summary>
        [Test]
        public async Task MarkNoticeSeenAsync_PersistsAcrossInstances()
        {
            RemoteNoticeService service = await CreateAsync(Json(noticeId: NOTICE_ID));

            await service.MarkNoticeSeenAsync(NOTICE_ID);

            RemoteNoticeService reloaded = await CreateAsync(Json(noticeId: NOTICE_ID));

            Assert.IsFalse(reloaded.TryGetNotice(NOW, out _));
        }
        #endregion


        #region Private Methods
        private async Task<RemoteNoticeService> CreateAsync(string json, string currentVersion = CURRENT_VERSION)
        {
            _source.Json = json;

            RemoteConfigService remoteConfig = new RemoteConfigService(_source);
            await remoteConfig.FetchAsync();

            RemoteNoticeService service = new RemoteNoticeService(remoteConfig, _repository, currentVersion);
            await service.LoadAsync();

            return service;
        }

        private static string Json(string latestVersion = "0.1.3", string noticeId = "", string from = "", string until = "")
        {
            return "{\"schemaVersion\":" + GameConstants.Remote.SCHEMA_VERSION
                 + ",\"latestVersion\":\"" + latestVersion + "\""
                 + ",\"notice\":{\"id\":\"" + noticeId + "\",\"title\":\"Title\",\"body\":\"Body\",\"from\":\"" + from + "\",\"until\":\"" + until + "\"}}";
        }
        #endregion
    }
}