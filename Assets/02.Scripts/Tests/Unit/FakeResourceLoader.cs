using CodeJelly.BounceOrBreak.Core.Resource;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// 테스트용 <see cref="IResourceLoader"/> 구현체.
    /// </summary>
    internal class FakeResourceLoader : IResourceLoader
    {
        #region Nested Classes
        /// <summary>
        /// 테스트가 완료시키는 <see cref="IResourceHandle"/> 구현체.
        /// </summary>
        internal sealed class FakeHandle : IResourceHandle
        {
            public UniTask<Object> Task => _source.Task;
            public int ReleaseCount { get; private set; }

            private readonly UniTaskCompletionSource<Object> _source = new UniTaskCompletionSource<Object>();

            public void Complete(Object asset)
            {
                _source.TrySetResult(asset);
            }

            public void Release()
            {
                ReleaseCount++;
            }
        }
        #endregion


        #region Properties
        public List<FakeHandle> Handles { get; } = new List<FakeHandle>();
        public FakeHandle LastHandle { get; private set; }
        public int LoadCount => Handles.Count;
        #endregion


        #region Public Methods
        /// <summary>
        /// IResourceLoader.InitializeAsync Implementation
        /// </summary>
        public UniTask<bool> InitializeAsync()
        {
            return UniTask.FromResult(true);
        }

        /// <summary>
        /// IResourceLoader.Load Implementation
        /// </summary>
        public IResourceHandle Load<T>(string key) where T : Object
        {
            LastHandle = new FakeHandle();
            Handles.Add(LastHandle);

            return LastHandle;
        }
        #endregion
    }
}