using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Bootstrap;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.Scope
{
    /// <summary>
    /// EntryScene의 LifetimeScope.
    /// </summary>
    public class EntrySceneScope : LifetimeScope
    {
        #region Private Variables
        /// <summary>
        /// EntryScene에서 사용하는 Transition 설정.
        /// 이 설정 자체가 Addressables 시스템 초기화 이전에 필요하므로,
        /// ResourceManager를 통한 로드가 불가능하여 직접 참조. (EntryScene 특수성)
        /// </summary>
        [SerializeField] private SceneTransitionConfig _entrySceneConfig;
        #endregion


        #region Protected Methods
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_entrySceneConfig);
            builder.RegisterEntryPoint<Bootstrapper>();
        }
        #endregion
    }
}
