using CodeJelly.BounceOrBreak.Core.Settings;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace CodeJelly.BounceOrBreak.Tests
{
    /// <summary>
    /// 테스트용 인메모리 <see cref="ISettingsRepository"/> 구현체.
    /// </summary>
    internal class FakeSettingsRepository : ISettingsRepository
    {
        #region Private Variables
        private readonly Dictionary<string, object> _store = new Dictionary<string, object>();
        #endregion


        #region Public Methods
        /// <summary>
        /// ISettingsRepository.LoadAsync Implementation
        /// </summary>
        public UniTask<SettingsResult<T>> LoadAsync<T>(string key) where T : class, new()
        {
            if (_store.TryGetValue(key, out object data) == true && data is T typed)
            {
                return UniTask.FromResult(SettingsResult<T>.Success(typed));
            }

            return UniTask.FromResult(SettingsResult<T>.Failure(SettingsResultError.NotFound));
        }

        /// <summary>
        /// ISettingsRepository.SaveAsync Implementation
        /// </summary>
        public UniTask<SettingsResult> SaveAsync<T>(string key, T data) where T : class
        {
            _store[key] = data;

            return UniTask.FromResult(SettingsResult.Success());
        }

        /// <summary>
        /// ISettingsRepository.DeleteAsync Implementation
        /// </summary>
        public UniTask<SettingsResult> DeleteAsync(string key)
        {
            _store.Remove(key);

            return UniTask.FromResult(SettingsResult.Success());
        }
        #endregion
    }
}
