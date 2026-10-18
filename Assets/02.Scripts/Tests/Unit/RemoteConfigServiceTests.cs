using CodeJelly.BounceOrBreak.Core.Remote;
using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.Remote;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// RemoteConfigService의 수신·무시 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class RemoteConfigServiceTests
    {
        #region Private Variables
        private FakeRemoteConfigSource _source;
        private RemoteConfigService _service;
        #endregion


        #region Public Methods
        [SetUp]
        public void SetUp()
        {
            _source = new FakeRemoteConfigSource();
            _service = new RemoteConfigService(_source);
        }

        /// <summary>
        /// 지원하는 스키마면 설정을 그대로 노출합니다.
        /// </summary>
        [Test]
        public async Task FetchAsync_SupportedSchema_ExposesConfig()
        {
            _source.Json = "{\"schemaVersion\":1,\"latestVersion\":\"0.1.4\"}";

            await _service.FetchAsync();

            Assert.IsNotNull(_service.Config);
            Assert.AreEqual("0.1.4", _service.Config.LatestVersion);
            Assert.AreEqual(GameConstants.Remote.CONFIG_URL, _source.LastUrl);
        }

        /// <summary>
        /// 수신에 실패하면 Config는 null입니다.
        /// </summary>
        [TestCase(RemoteResultError.Timeout)]
        [TestCase(RemoteResultError.NetworkError)]
        [TestCase(RemoteResultError.HttpError)]
        [TestCase(RemoteResultError.SerializationError)]
        public async Task FetchAsync_SourceFails_LeavesConfigNull(RemoteResultError error)
        {
            _source.Error = error;

            await _service.FetchAsync();

            Assert.IsNull(_service.Config);
        }

        /// <summary>
        /// 지원하지 않는 스키마 버전이면 설정 전체를 무시합니다.
        /// </summary>
        [Test]
        public async Task FetchAsync_UnsupportedSchema_IgnoresConfig()
        {
            _source.Json = "{\"schemaVersion\":" + (GameConstants.Remote.SCHEMA_VERSION + 1) + ",\"latestVersion\":\"9.9.9\"}";

            await _service.FetchAsync();

            Assert.IsNull(_service.Config);
        }

        /// <summary>
        /// JSON에 공지가 없으면 Notice는 null입니다.
        /// </summary>
        [Test]
        public async Task FetchAsync_NoNoticeField_LeavesNoticeNull()
        {
            _source.Json = "{\"schemaVersion\":1,\"latestVersion\":\"0.1.4\"}";

            await _service.FetchAsync();

            Assert.IsNull(_service.Config.Notice);
        }
        #endregion
    }
}