using CodeJelly.BounceOrBreak.Core.Logging;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CodeJelly.BounceOrBreak.Core.Resource
{
    /// <summary>
    /// Addressables 기반 <see cref="IResourceLoader"/> 구현체.
    /// </summary>
    public class AddressablesResourceLoader : IResourceLoader
    {
        #region Nested Classes
        /// <summary>
        /// Addressables 비동기 작업 핸들을 감싼 <see cref="IResourceHandle"/> 구현체.
        /// </summary>
        private sealed class Handle : IResourceHandle
        {
            #region Constructors
            public Handle(AsyncOperationHandle handle, string key)
            {
                _handle = handle;
                Task = WaitAsync(key);
            }
            #endregion


            #region Properties
            /// <summary>
            /// IResourceHandle.Task Implementation
            /// </summary>
            public UniTask<UnityEngine.Object> Task { get; }
            #endregion


            #region Private Variables
            private readonly AsyncOperationHandle _handle;

            private bool _released;
            #endregion


            #region Public Methods
            /// <summary>
            /// IResourceHandle.Release Implementation
            /// </summary>
            public void Release()
            {
                if (_released == true)
                {
                    return;
                }

                _released = true;

                if (_handle.IsValid() == true)
                {
                    Addressables.Release(_handle);
                }
            }
            #endregion


            #region Private Methods
            private async UniTask<UnityEngine.Object> WaitAsync(string key)
            {
                try
                {
                    await _handle.ToUniTask();

                    // 해제된 뒤 끝난 로드는 결과를 쓰지 않음
                    if (_released == true)
                    {
                        return null;
                    }

                    return _handle.Result as UnityEngine.Object;
                }
                catch (Exception ex)
                {
                    if (_released == false)
                    {
                        AppLogger.Exception(ex, $"[AddressablesResourceLoader] Load Failed: {key}");
                    }

                    return null;
                }
            }
            #endregion
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// IResourceLoader.InitializeAsync Implementation
        /// </summary>
        public async UniTask<bool> InitializeAsync()
        {
            AppLogger.Info("[AddressablesResourceLoader] Initializing Addressables system...");

            var handle = Addressables.InitializeAsync();
            await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                AppLogger.Info("[AddressablesResourceLoader] Addressables system initialized successfully.");
                return true;
            }
            else
            {
                AppLogger.Error($"[AddressablesResourceLoader] Failed to initialize Addressables. Status: {handle.Status}");
                return false;
            }
        }

        /// <summary>
        /// IResourceLoader.Load Implementation
        /// </summary>
        public IResourceHandle Load<T>(string key) where T : UnityEngine.Object
        {
            return new Handle(Addressables.LoadAssetAsync<T>(key), key);
        }
        #endregion
    }
}