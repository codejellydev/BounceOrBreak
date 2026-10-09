using System;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.UI
{
    /// <summary>
    /// 루트 Canvas의 Sort Order를 계층에 맞추는 클래스.
    /// 순서는 <see cref="GameConstants.UI.CanvasLayers.ORDER"/>를 따릅니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class CanvasLayerSorter : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// CanvasLayerSorter가 유효한지 확인합니다.
        /// </summary>
        public bool IsValid
        {
            get
            {
                Canvas canvas = GetComponent<Canvas>();

                return canvas != null
                    && canvas.isRootCanvas == true
                    && TryGetSortingOrder(out int _) == true;
            }
        }
        #endregion


        #region Private Variables
        [Tooltip("Canvas가 속하는 계층")]
        [SerializeField] private CanvasLayer _layer = CanvasLayer.Scene;

        private Canvas _canvas;
        #endregion


        #region Unity Lifecycle
        private void OnEnable()
        {
            InitializeComponents();
            Apply();

            Canvas.preWillRenderCanvases += Apply;
        }

        private void OnDisable()
        {
            Canvas.preWillRenderCanvases -= Apply;
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            // ExecuteAlways 컴포넌트는 도메인 리로드 뒤 Awake 없이 OnEnable만 다시 불리므로 여기서 참조를 잡음
            _canvas ??= GetComponent<Canvas>();
        }

        /// <summary>
        /// Canvas의 Sort Order를 갱신합니다.
        /// </summary>
        private void Apply()
        {
            // 중첩된 Canvas는 대상이 아님
            if (_canvas.isRootCanvas == false)
            {
                return;
            }

            // ORDER에 선언되지 않은 계층은 순서를 구할 수 없음
            if (TryGetSortingOrder(out int sortingOrder) == false)
            {
                return;
            }

            // Canvas를 그리기 직전마다 불리므로 값이 바뀔 때만 넣음
            if (_canvas.sortingOrder == sortingOrder)
            {
                return;
            }

            _canvas.sortingOrder = sortingOrder;
        }

        /// <summary>
        /// 계층에 해당하는 Sort Order를 반환합니다.
        /// </summary>
        private bool TryGetSortingOrder(out int sortingOrder)
        {
            sortingOrder = Array.IndexOf(GameConstants.UI.CanvasLayers.ORDER, _layer);

            return sortingOrder >= 0;
        }
        #endregion
    }
}