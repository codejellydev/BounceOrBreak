using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Pooling;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CodeJelly.BounceOrBreak.Core.Resource
{
    /// <summary>
    /// Addressables로 로드된 단일 리소스의 생명주기를 관리하는 래퍼 클래스.
    /// ObjectPool을 통해 재사용되며, 참조 카운팅 방식으로 메모리를 관리합니다.
    /// </summary>
    internal class ResourceNode : IPoolable
    {
        #region Properties
        /// <summary>
        /// Addressables 비동기 작업 핸들.
        /// </summary>
        public AsyncOperationHandle Handle { get; private set; }

        /// <summary>
        /// Addressables 리소스의 키.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// 현재 리소스를 참조하고 있는 개수.
        /// </summary>
        public int ReferenceCount { get; private set; }
        #endregion


        #region Public Methods
        /// <summary>
        /// ResourceNode를 초기화합니다.
        /// </summary>
        public void Initialize(AsyncOperationHandle handle, string key)
        {
            Handle = handle;
            Key = key;
            ReferenceCount = 1;
        }

        /// <summary>
        /// 로드된 에셋을 반환합니다.
        /// </summary>
        public T GetAsset<T>() where T : Object
        {
            var result = Handle.Result as T;

            if (result == null && Handle.Result != null)
            {
                AppLogger.Error($"[ResourceNode] Type mismatch: {Key} (Expected: {typeof(T).Name}, Actual: {Handle.Result.GetType().Name})");
            }

            return result;
        }

        /// <summary>
        /// 참조 카운트를 증가시킵니다.
        /// </summary>
        public void IncrementReference()
        {
            ReferenceCount++;
        }

        /// <summary>
        /// 참조 카운트를 감소시킵니다.
        /// </summary>
        public void DecrementReference()
        {
            ReferenceCount--;
        }

        /// <summary>
        /// IPoolable.Get Implementation
        /// </summary>
        public void Get()
        {
            ReferenceCount = 0;
        }

        /// <summary>
        /// IPoolable.Release Implementation
        /// </summary>
        public void Release()
        {
            Handle = default;
            Key = null;
            ReferenceCount = 0;
        }

        /// <summary>
        /// IPoolable.Destroy Implementation
        /// </summary>
        public void Destroy()
        {
            Handle = default;
            Key = null;
        }
        #endregion
    }
}