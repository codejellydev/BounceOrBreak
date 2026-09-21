using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Core.Resource;
using CodeJelly.BounceOrBreak.Core.Scene;
using CodeJelly.BounceOrBreak.Game.Input;
using CodeJelly.BounceOrBreak.Game.UI.Panel;
using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CodeJelly.BounceOrBreak.Game.UI.Navigation
{
    /// <summary>
    /// UI 패널을 열고 뒤로가기를 중재하는 애플리케이션 수준 서비스.
    /// 패널 인스턴스는 씬에 속하며, 서비스는 View를 소유하지 않습니다.
    /// </summary>
    public class UINavigationService : IStartable, IDisposable
    {
        #region Nested Classes
        /// <summary>
        /// 루트 등록의 해제용 IDisposable. Dispose로 등록을 해제하고 호스트를 정리합니다.
        /// </summary>
        private sealed class RootRegistration : IDisposable
        {
            #region Constructors
            public RootRegistration(List<RootRegistration> owner, UIPanelHost host)
            {
                _owner = owner;
                Host = host;
            }
            #endregion


            #region Properties
            public UIPanelHost Host { get; }
            #endregion


            #region Private Variables
            private readonly List<RootRegistration> _owner;
            #endregion


            #region Public Methods
            /// <summary>
            /// IDisposable.Dispose Implementation
            /// </summary>
            public void Dispose()
            {
                _owner.Remove(this);
                Host.Dispose();
            }
            #endregion
        }
        #endregion


        #region Injection
        private readonly InputManager _inputManager;
        private readonly SceneLoader _sceneLoader;
        private readonly ResourceManager _resourceManager;
        private readonly UIPanelConfig _config;

        public UINavigationService(
            InputManager inputManager,
            SceneLoader sceneLoader,
            ResourceManager resourceManager,
            UIPanelConfig config)
        {
            _inputManager = inputManager;
            _sceneLoader = sceneLoader;
            _resourceManager = resourceManager;
            _config = config;
        }
        #endregion


        #region Private Variables
        private readonly BackHandlerStack _backHandlers = new BackHandlerStack();

        /// <summary>
        /// Additive 전환 중엔 새 씬과 이전 씬의 루트가 잠시 공존하므로 목록으로 두고 마지막을 현재로 봄
        /// </summary>
        private readonly List<RootRegistration> _roots = new List<RootRegistration>();
        private readonly HashSet<string> _showingKeys = new HashSet<string>();
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        #endregion


        #region VContainer Lifecycle
        public void Start()
        {
            _inputManager.OnBackPressed
                         .Subscribe(_ => OnBackPressed())
                         .AddTo(_disposables);
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// 씬의 UIPanelRoot를 등록하고 Persistent 패널을 미리 로드합니다.
        /// </summary>
        /// <returns>등록 해제용 IDisposable</returns>
        public IDisposable RegisterRoot(UIPanelRoot root, IReadOnlyList<IUIPanel> panels, IObjectResolver resolver)
        {
            UIPanelHost host = new UIPanelHost(root.transform, resolver, panels, _resourceManager);
            RootRegistration registration = new RootRegistration(_roots, host);
            _roots.Add(registration);

            PreloadPersistent(host, panels).Forget();

            return registration;
        }

        /// <summary>
        /// 뒤로가기 핸들러를 맨 위에 올립니다.
        /// </summary>
        /// <returns>Dispose하면 스택에서 빠짐</returns>
        public IDisposable PushBackHandler(Action handler)
        {
            return _backHandlers.Push(handler);
        }

        /// <summary>
        /// 결과가 없는 패널을 열고 닫힐 때까지 기다립니다.
        /// </summary>
        public async UniTask ShowAsync(string key, CancellationToken ct)
        {
            await ShowAsync<Unit>(key, ct);
        }

        /// <summary>
        /// 패널을 열고 결과를 기다립니다.
        /// </summary>
        /// <param name="key">패널 키</param>
        /// <param name="ct">씬 스코프 토큰. 씬이 먼저 끝나면 OperationCanceledException</param>
        public async UniTask<TResult> ShowAsync<TResult>(string key, CancellationToken ct)
        {
            UIPanelHost host = GetCurrentHost();
            UIPanelConfig.Entry entry = GetEntry(key);
            IUIPanel<TResult> panel = host.GetPanel<TResult>(key);

            // 중복 열기는 정상 닫힘과 같으므로 CancelResult로 응답
            if (_showingKeys.Add(key) == false)
            {
                AppLogger.Debug($"[UINavigationService] '{key}' is already showing.");

                return panel.CancelResult;
            }

            try
            {
                return await OpenAsync(host, entry, panel, ct);
            }
            finally
            {
                _showingKeys.Remove(key);
            }
        }

        /// <summary>
        /// IDisposable.Dispose Implementation
        /// </summary>
        public void Dispose()
        {
            _disposables.Dispose();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 현재 루트(마지막 등록)의 호스트를 반환합니다.
        /// </summary>
        private UIPanelHost GetCurrentHost()
        {
            if (_roots.Count == 0)
            {
                throw new InvalidOperationException("[UINavigationService] No UIPanelRoot is registered.");
            }

            return _roots[^1].Host;
        }

        /// <summary>
        /// 키의 선언을 반환합니다.
        /// </summary>
        private UIPanelConfig.Entry GetEntry(string key)
        {
            if (_config.TryGetEntry(key, out UIPanelConfig.Entry entry) == false)
            {
                throw new InvalidOperationException($"[UINavigationService] '{key}' is not declared in {_config.name}.");
            }

            return entry;
        }

        /// <summary>
        /// 패널을 열고 결과를 기다립니다.
        /// </summary>
        private async UniTask<TResult> OpenAsync<TResult>(UIPanelHost host, UIPanelConfig.Entry entry, IUIPanel<TResult> panel, CancellationToken ct)
        {
            using (CancellationTokenSource showCts = CancellationTokenSource.CreateLinkedTokenSource(ct, host.LifetimeToken))
            {
                GameObject instance;

                // 로드 중의 뒤로가기는 열기를 취소
                using (_backHandlers.Push(showCts.Cancel))
                {
                    try
                    {
                        instance = await host.AcquireAsync(entry, showCts.Token);
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested == false && host.LifetimeToken.IsCancellationRequested == false)
                    {
                        // 뒤로가기 취소는 정상 닫힘이므로 CancelResult로 응답
                        return panel.CancelResult;
                    }
                }

                instance.SetActive(true);

                // 열린 뒤의 뒤로가기는 패널의 취소
                using (_backHandlers.Push(panel.Cancel))
                {
                    try
                    {
                        return await panel.RunAsync(instance, showCts.Token);
                    }
                    finally
                    {
                        host.Release(entry, instance);
                    }
                }
            }
        }

        /// <summary>
        /// 이 루트의 Persistent 패널을 미리 로드합니다.
        /// </summary>
        private async UniTaskVoid PreloadPersistent(UIPanelHost host, IReadOnlyList<IUIPanel> panels)
        {
            foreach (IUIPanel panel in panels)
            {
                if (_config.TryGetEntry(panel.Key, out UIPanelConfig.Entry entry) == false)
                {
                    AppLogger.Error($"[UINavigationService] '{panel.Key}' is registered in the scene but not declared in {_config.name}.");

                    continue;
                }

                if (entry.Lifetime != UIPanelLifetime.Persistent)
                {
                    continue;
                }

                try
                {
                    await host.AcquireAsync(entry, host.LifetimeToken);
                }
                catch (OperationCanceledException)
                {
                    // 루트가 해제된 것이므로 중단
                    return;
                }
            }
        }

        /// <summary>
        /// 뒤로가기를 처리합니다.
        /// </summary>
        private void OnBackPressed()
        {
            // 전환 중엔 이전 씬 핸들러가 아직 남아 있을 수 있으므로 무시
            if (_sceneLoader.IsTransitioning == true)
            {
                return;
            }

            try
            {
                if (_backHandlers.Handle() == false)
                {
                    AppLogger.Debug("[UINavigationService] Back pressed with no handler.");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Exception(ex, "[UINavigationService] Back handler threw.");
            }
        }
        #endregion
    }
}