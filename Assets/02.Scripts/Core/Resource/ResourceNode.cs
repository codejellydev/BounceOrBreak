using CodeJelly.BounceOrBreak.Core.Logging;
using Cysharp.Threading.Tasks;
using System;

namespace CodeJelly.BounceOrBreak.Core.Resource
{
    /// <summary>
    /// 리소스 하나의 로드 상태와 참조 카운트를 관리하는 클래스.
    /// </summary>
    internal class ResourceNode
    {
        #region Constructors
        public ResourceNode(IResourceHandle handle, string key)
        {
            _handle = handle;
            Key = key;
            ReferenceCount = 1;

            WaitAsync().Forget();
        }
        #endregion


        #region Properties
        /// <summary>
        /// 로드 완료를 기다리는 작업.
        /// </summary>
        public UniTask Task => _completion.Task;

        /// <summary>
        /// 리소스의 키.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 현재 리소스를 참조하고 있는 개수.
        /// </summary>
        public int ReferenceCount { get; private set; }

        /// <summary>
        /// 로드 실패 여부.
        /// </summary>
        public bool IsFailed { get; private set; }
        #endregion


        #region Private Variables
        private readonly IResourceHandle _handle;

        /// <summary>
        /// 핸들의 작업은 한 번만 await할 수 있으므로 완료를 중계해 여러 대기자에게 전파
        /// </summary>
        private readonly UniTaskCompletionSource _completion = new UniTaskCompletionSource();

        private UnityEngine.Object _asset;
        #endregion


        #region Public Methods
        /// <summary>
        /// 로드된 에셋을 반환합니다.
        /// </summary>
        public T GetAsset<T>() where T : UnityEngine.Object
        {
            T result = _asset as T;

            if (result == null && _asset != null)
            {
                AppLogger.Error($"[ResourceNode] Type mismatch: {Key} (Expected: {typeof(T).Name}, Actual: {_asset.GetType().Name})");
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
        /// 리소스를 해제합니다.
        /// </summary>
        public void Release()
        {
            _handle.Release();
        }
        #endregion


        #region Private Methods
        private async UniTaskVoid WaitAsync()
        {
            try
            {
                _asset = await _handle.Task;
            }
            catch (Exception ex)
            {
                AppLogger.Exception(ex, $"[ResourceNode] Load Failed: {Key}");
            }

            IsFailed = _asset == null;
            _completion.TrySetResult();
        }
        #endregion
    }
}