using CodeJelly.BounceOrBreak.Game.Trampoline;
using NUnit.Framework;
using System.Collections.Generic;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// Trampoline 관련 에셋 데이터의 유효성을 검증하는 테스트 클래스.
    /// </summary>
    public class TrampolineAssetValidationTests
    {
        #region Public Methods
        /// <summary>
        /// TrampolineRegistry의 Entry 목록이 유효합니다.
        /// </summary>
        [Test]
        public void TrampolineRegistry_HasNoInvalidEntries()
        {
            List<TrampolineRegistry> registries = new List<TrampolineRegistry>(EditorAssetLoader.LoadAll<TrampolineRegistry>());

            // 에셋을 못 찾으면 무조건 통과하므로 로드 자체를 검증
            Assert.IsNotEmpty(registries, "No TrampolineRegistry asset found.");

            foreach (TrampolineRegistry registry in registries)
            {
                HashSet<TrampolineType> types = new HashSet<TrampolineType>();

                foreach (TrampolineEntry entry in registry.Entries)
                {
                    Assert.IsTrue(types.Add(entry.Type), $"Duplicate TrampolineType in {registry.name}: {entry.Type}.");

                    // AssetReference는 배선되지 않아도 객체 자체는 존재하므로 RuntimeKey를 확인하는 IsValid를 사용
                    Assert.IsTrue(entry.IsValid, $"Prefab AssetReference is invalid for {entry.Type} in {registry.name}.");
                }
            }
        }
        #endregion
    }
}
