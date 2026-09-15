using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Core.Viewport
{
    /// <summary>
    /// ViewportConfig가 보장하는 영역이 항상 보이도록 Camera를 맞추는 클래스.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class ViewportFitter : MonoBehaviour
    {
        #region Private Variables
        [SerializeField] private ViewportConfig _config;
        [SerializeField] private Camera _camera;
        #endregion


        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
        }

        private void Update()
        {
            if (_config == null)
            {
                return;
            }

            ApplyFit(_config.Contract);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            // 직렬화 참조는 미할당이어도 C# null이 아니므로 ??= 로는 감지되지 않음
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            // 에디터에서는 인스펙터와 Game 뷰로 확인할 수 있으므로 플레이 중에만 알림
            if (Application.isPlaying == false)
            {
                return;
            }

            if (_config == null)
            {
                AppLogger.Error("[ViewportFitter] Config ScriptableObject is missing!");
            }

            // Perspective면 orthographicSize가 렌더링에 반영되지 않아 보장이 무너짐
            if (_camera.orthographic == false)
            {
                AppLogger.Warning("[ViewportFitter] Camera projection is not orthographic. The guaranteed area is not applied.");
            }
        }

        /// <summary>
        /// 현재 화면비에 맞춰 Camera를 갱신합니다.
        /// </summary>
        private void ApplyFit(ViewportContract contract)
        {
            float aspect = _camera.aspect;
            ViewportFit fit = contract.Resolve(aspect);

            // 영역은 월드 원점을 가로 중심으로 하므로 Camera의 x는 항상 0
            Vector3 targetPosition = new Vector3(0f, fit.CenterY, transform.position.z);

            // 값이 이미 같은데도 대입하면 에디터에서 씬이 변경된 것으로 처리되므로 건너뜀
            if (Mathf.Approximately(_camera.orthographicSize, fit.OrthographicSize) == true
                && transform.position == targetPosition)
            {
                return;
            }

            _camera.orthographicSize = fit.OrthographicSize;
            transform.position = targetPosition;

            LogApplied(aspect, fit, contract.MinAspect);
        }

        /// <summary>
        /// 적용 결과를 기록합니다.
        /// </summary>
        private void LogApplied(float aspect, ViewportFit fit, float minAspect)
        {
            // 에디터에서는 뷰 크기를 조절할 때마다 호출되므로 로그를 남기지 않음
            if (Application.isPlaying == false)
            {
                return;
            }

            if (aspect < minAspect)
            {
                AppLogger.Warning($"[ViewportFitter] Aspect {aspect:F3} is below the guaranteed minimum {minAspect:F3}. Offscreen elements may become visible.");
            }

            AppLogger.Debug($"[ViewportFitter] Applied. aspect={aspect:F3}, size={fit.OrthographicSize:F2}, centerY={fit.CenterY:F2}.");
        }
        #endregion


        #region Debug
#if UNITY_EDITOR
        /// <summary>
        /// 씬 뷰에서 보장 영역과 화면 밖 배치 기준선을 시각화합니다.
        /// 기준선보다 위에 있는 요소들은 모든 지원 화면비에서 화면 밖에 있습니다.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (_config == null)
            {
                return;
            }

            ViewportContract contract = _config.Contract;

            // 모든 화면비에서 보이는 것이 보장되는 영역
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(contract.HalfWidth * 2f, contract.HalfHeight * 2f, 0f));

            // 최소 화면비에서의 화면 상단. 화면 밖이어야 하는 요소는 이 선보다 위에 배치
            float limitTopY = contract.ResolveMinAspect().TopY;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                new Vector3(-contract.HalfWidth, limitTopY, 0f),
                new Vector3(contract.HalfWidth, limitTopY, 0f));
        }
#endif
        #endregion
    }
}