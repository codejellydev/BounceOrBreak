using System;
using UnityEngine.Pool;

namespace CodeJelly.BounceOrBreak.Core.Pooling
{
    /// <summary>
    /// IPoolable 객체에 대한 ObjectPool 헬퍼 클래스.
    /// </summary>
    public static class PoolExtensions
    {
        #region Public Methods
        /// <summary>
        /// IPoolable 객체를 위한 ObjectPool을 생성합니다.
        /// </summary>
        public static ObjectPool<T> CreatePoolablePool<T>
        (
            Func<T> createFunc,
            int defaultCapacity,
            int maxSize
        ) where T : class, IPoolable
        {
            return new ObjectPool<T>
            (
                // 신규 인스턴스도 Release를 거쳐 재사용 객체와 동일한 풀 상태로 편입
                createFunc: () =>
                {
                    T item = createFunc();
                    if (item == null)
                    {
                        throw new InvalidOperationException($"[PoolExtensions] createFunc returned null. (Type: {typeof(T).Name})");
                    }

                    item.Release();

                    return item;
                },
                actionOnGet: item => item.Get(),
                actionOnRelease: item => item.Release(),
                actionOnDestroy: item => item.Destroy(),
                collectionCheck: true, // 이미 Release된 오브젝트를 다시 Release 시도하는 경우 exception 발생
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }
        #endregion
    }
}