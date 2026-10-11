using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.Settings.Graphics;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// GraphicsSettingsService의 불러오기·적용·저장을 검증하는 테스트 클래스.
    /// </summary>
    public class GraphicsSettingsServiceTests
    {
        #region Nested Classes
        /// <summary>
        /// 적용 호출을 기록하는 테스트용 IGraphicsSettingsApplier 구현체.
        /// </summary>
        private sealed class FakeGraphicsSettingsApplier : IGraphicsSettingsApplier
        {
            #region Properties
            public GraphicsSettings AppliedSettings { get; private set; }
            public int QualityApplyCount { get; private set; }
            #endregion


            #region Public Methods
            /// <summary>
            /// IGraphicsSettingsApplier.ApplyQualityLevel Implementation
            /// </summary>
            public void ApplyQualityLevel(int qualityLevel)
            {
                QualityApplyCount++;
            }

            /// <summary>
            /// IGraphicsSettingsApplier.ApplyTargetFrameRate Implementation
            /// </summary>
            public void ApplyTargetFrameRate(int targetFrameRate)
            { }

            /// <summary>
            /// IGraphicsSettingsApplier.ApplySettings Implementation
            /// </summary>
            public void ApplySettings(GraphicsSettings settings)
            {
                AppliedSettings = settings;
            }
            #endregion
        }
        #endregion


        #region Private Variables
        private FakeSettingsRepository _repository;
        private FakeGraphicsSettingsApplier _applier;
        private GraphicsSettingsService _service;
        #endregion


        #region Public Methods
        [SetUp]
        public void SetUp()
        {
            _repository = new FakeSettingsRepository();
            _applier = new FakeGraphicsSettingsApplier();
            _service = new GraphicsSettingsService(_applier, _repository);
        }

        /// <summary>
        /// 저장된 설정이 없으면 기본값을 적용합니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_NoSavedSettings_AppliesDefaults()
        {
            await _service.LoadAsync();

            Assert.AreEqual(GameConstants.Graphics.Quality.DEFAULT, _service.Settings.QualityLevel);
            Assert.AreSame(_service.Settings, _applier.AppliedSettings);
        }

        /// <summary>
        /// 저장된 설정이 있으면 불러와 적용합니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_SavedSettingsExist_LoadsAndApplies()
        {
            await _repository.SaveAsync(GameConstants.Graphics.Keys.REPOSITORY_KEY, new GraphicsSettings { QualityLevel = GameConstants.Graphics.Quality.HIGH });
            await _service.LoadAsync();

            Assert.AreEqual(GameConstants.Graphics.Quality.HIGH, _service.Settings.QualityLevel);
            Assert.AreSame(_service.Settings, _applier.AppliedSettings);
        }

        /// <summary>
        /// 품질 수준을 바꾸면 적용합니다.
        /// </summary>
        [Test]
        public void SetQualityLevel_NewValue_Applies()
        {
            _service.SetQualityLevel(GameConstants.Graphics.Quality.HIGH);

            Assert.AreEqual(GameConstants.Graphics.Quality.HIGH, _service.Settings.QualityLevel);
            Assert.AreEqual(1, _applier.QualityApplyCount);
        }

        /// <summary>
        /// 같은 품질 수준이면 다시 적용하지 않습니다.
        /// </summary>
        [Test]
        public void SetQualityLevel_SameValue_DoesNotApply()
        {
            _service.SetQualityLevel(_service.Settings.QualityLevel);

            Assert.AreEqual(0, _applier.QualityApplyCount);
        }

        /// <summary>
        /// 저장하면 현재 설정이 저장소에 남습니다.
        /// </summary>
        [Test]
        public async Task SaveAsync_PersistsCurrentSettings()
        {
            _service.SetQualityLevel(GameConstants.Graphics.Quality.LOW);
            await _service.SaveAsync();

            var result = await _repository.LoadAsync<GraphicsSettings>(GameConstants.Graphics.Keys.REPOSITORY_KEY);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(GameConstants.Graphics.Quality.LOW, result.Value.QualityLevel);
        }
        #endregion
    }
}