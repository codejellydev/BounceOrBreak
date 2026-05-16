using CodeJelly.BounceOrBreak.Core.Logging;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Core.Settings
{
    /// <summary>
    /// PlayerPrefs 기반 <see cref="ISettingsRepository"/> 구현체.
    /// <para>
    /// PlayerPrefs 자체는 동기 API이지만 인터페이스 일관성을 위해 UniTask로 래핑합니다.
    /// </para>
    /// </summary>
    public class PlayerPrefsSettingsRepository : ISettingsRepository
    {
        #region Public Methods
        /// <summary>
        /// ISettingsRepository.LoadAsync Implementation
        /// </summary>
        public UniTask<SettingsResult<T>> LoadAsync<T>(string key) where T : class, new()
        {
            if (PlayerPrefs.HasKey(key) == false)
            {
                return UniTask.FromResult(SettingsResult<T>.Failure(SettingsResultError.NotFound));
            }

            try
            {
                string json = PlayerPrefs.GetString(key);
                T data = JsonUtility.FromJson<T>(json);
                return UniTask.FromResult(SettingsResult<T>.Success(data));
            }
            catch (Exception ex)
            {
                AppLogger.Exception(ex, $"[PlayerPrefsSettingsRepository] Failed to load settings for key: {key}");
                return UniTask.FromResult(SettingsResult<T>.Failure(SettingsResultError.SerializationError));
            }
        }

        /// <summary>
        /// ISettingsRepository.SaveAsync Implementation
        /// </summary>
        public UniTask<SettingsResult> SaveAsync<T>(string key, T data) where T : class
        {
            if (data == null)
            {
                AppLogger.Warning($"[PlayerPrefsSettingsRepository] Attempted to save null data for key: {key}");
                return UniTask.FromResult(SettingsResult.Failure(SettingsResultError.InvalidData));
            }

            try
            {
                string json = JsonUtility.ToJson(data);
                PlayerPrefs.SetString(key, json);
                PlayerPrefs.Save();
                return UniTask.FromResult(SettingsResult.Success());
            }
            catch (Exception ex)
            {
                AppLogger.Exception(ex, $"[PlayerPrefsSettingsRepository] Failed to save settings for key: {key}");
                return UniTask.FromResult(SettingsResult.Failure(SettingsResultError.SerializationError));
            }
        }

        /// <summary>
        /// ISettingsRepository.DeleteAsync Implementation
        /// </summary>
        public UniTask<SettingsResult> DeleteAsync(string key)
        {
            try
            {
                if (PlayerPrefs.HasKey(key) == true)
                {
                    PlayerPrefs.DeleteKey(key);
                    PlayerPrefs.Save();
                }
                return UniTask.FromResult(SettingsResult.Success());
            }
            catch (Exception ex)
            {
                AppLogger.Exception(ex, $"[PlayerPrefsSettingsRepository] Failed to delete settings for key: {key}");
                return UniTask.FromResult(SettingsResult.Failure(SettingsResultError.IOError));
            }
        }
        #endregion
    }
}