using Cysharp.Threading.Tasks;

namespace CodeJelly.BounceOrBreak.Core.Settings
{
    /// <summary>
    /// 설정 데이터의 저장소 인터페이스.
    /// 구현체는 PlayerPrefs, File, Cloud 등 다양한 저장 방식의 설정 데이터를 관리 가능.
    /// </summary>
    public interface ISettingsRepository
    {
        /// <summary>
        /// 주어진 키에 해당하는 설정 데이터를 불러옵니다.
        /// </summary>
        UniTask<SettingsResult<T>> LoadAsync<T>(string key) where T : class, new();

        /// <summary>
        /// 주어진 키로 설정 데이터를 저장합니다.
        /// </summary>
        UniTask<SettingsResult> SaveAsync<T>(string key, T data) where T : class;

        /// <summary>
        /// 주어진 키의 데이터를 삭제합니다.
        /// </summary>
        UniTask<SettingsResult> DeleteAsync(string key);
    }
}