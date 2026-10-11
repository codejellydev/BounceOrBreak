using CodeJelly.BounceOrBreak.Game;
using CodeJelly.BounceOrBreak.Game.Settings.Language;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// LanguageSettingsService의 불러오기·적용·저장을 검증하는 테스트 클래스.
    /// </summary>
    public class LanguageSettingsServiceTests
    {
        #region Nested Classes
        /// <summary>
        /// 적용 호출을 기록하는 테스트용 ILanguageSettingsApplier 구현체.
        /// </summary>
        private sealed class FakeLanguageSettingsApplier : ILanguageSettingsApplier
        {
            #region Properties
            public LanguageSettings AppliedSettings { get; private set; }
            public string AppliedLocaleCode { get; private set; }
            public int LocaleApplyCount { get; private set; }
            #endregion


            #region Public Methods
            /// <summary>
            /// ILanguageSettingsApplier.ApplyLanguageAsync Implementation
            /// </summary>
            public UniTask ApplyLanguageAsync(string localeCode, CancellationToken cancellationToken = default)
            {
                AppliedLocaleCode = localeCode;
                LocaleApplyCount++;

                return UniTask.CompletedTask;
            }

            /// <summary>
            /// ILanguageSettingsApplier.ApplySettingsAsync Implementation
            /// </summary>
            public UniTask ApplySettingsAsync(LanguageSettings settings, CancellationToken cancellationToken = default)
            {
                AppliedSettings = settings;

                return UniTask.CompletedTask;
            }
            #endregion
        }
        #endregion


        #region Private Variables
        private const string OTHER_LOCALE_CODE = "ko";

        private FakeSettingsRepository _repository;
        private FakeLanguageSettingsApplier _applier;
        private LanguageSettingsService _service;
        #endregion


        #region Public Methods
        [SetUp]
        public void SetUp()
        {
            _repository = new FakeSettingsRepository();
            _applier = new FakeLanguageSettingsApplier();
            _service = new LanguageSettingsService(_applier, _repository);
        }

        [TearDown]
        public void TearDown()
        {
            _service.Dispose();
        }

        /// <summary>
        /// 저장된 설정이 없으면 기본값을 적용합니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_NoSavedSettings_AppliesDefaults()
        {
            await _service.LoadAsync();

            Assert.AreEqual(GameConstants.Language.LocaleCode.DEFAULT, _service.Settings.LocaleCode);
            Assert.AreSame(_service.Settings, _applier.AppliedSettings);
        }

        /// <summary>
        /// 저장된 설정이 있으면 불러와 적용합니다.
        /// </summary>
        [Test]
        public async Task LoadAsync_SavedSettingsExist_LoadsAndApplies()
        {
            await _repository.SaveAsync(GameConstants.Language.Keys.REPOSITORY_KEY, new LanguageSettings { LocaleCode = OTHER_LOCALE_CODE });
            await _service.LoadAsync();

            Assert.AreEqual(OTHER_LOCALE_CODE, _service.Settings.LocaleCode);
            Assert.AreSame(_service.Settings, _applier.AppliedSettings);
        }

        /// <summary>
        /// 로케일을 바꾸면 적용합니다.
        /// </summary>
        [Test]
        public void SetLocale_NewValue_Applies()
        {
            _service.SetLocale(OTHER_LOCALE_CODE);

            Assert.AreEqual(OTHER_LOCALE_CODE, _service.Settings.LocaleCode);
            Assert.AreEqual(OTHER_LOCALE_CODE, _applier.AppliedLocaleCode);
        }

        /// <summary>
        /// 같은 로케일이면 다시 적용하지 않습니다.
        /// </summary>
        [Test]
        public void SetLocale_SameValue_DoesNotApply()
        {
            _service.SetLocale(_service.Settings.LocaleCode);

            Assert.AreEqual(0, _applier.LocaleApplyCount);
        }

        /// <summary>
        /// 저장하면 현재 설정이 저장소에 남습니다.
        /// </summary>
        [Test]
        public async Task SaveAsync_PersistsCurrentSettings()
        {
            _service.SetLocale(OTHER_LOCALE_CODE);
            await _service.SaveAsync();

            var result = await _repository.LoadAsync<LanguageSettings>(GameConstants.Language.Keys.REPOSITORY_KEY);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(OTHER_LOCALE_CODE, result.Value.LocaleCode);
        }
        #endregion
    }
}