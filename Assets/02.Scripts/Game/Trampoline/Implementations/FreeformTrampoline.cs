using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// 그린 모양 그대로 유지되는 자유형 Trampoline.
    /// </summary>
    public class FreeformTrampoline : TrampolineBase
    {
        #region Properties
        /// <summary>
        /// ITrampoline.Type Implementation
        /// </summary>
        public override TrampolineType Type => TrampolineType.Freeform;
        #endregion


        #region Private Variables
        private Vector2[] _linePoints;
        #endregion


        #region Public Methods
        /// <summary>
        /// ITrampoline.Initialize Implementation
        /// </summary>
        /// <param name="drawer">Trampoline을 생성한 Drawer</param>
        /// <param name="config">Trampoline 설정</param>
        /// <param name="points">LineRenderer의 point 배열</param>
        public override void Initialize(TrampolineDrawer drawer, TrampolineConfig config, Vector2[] points)
        {
            _drawer = drawer;
            _config = config;
            _linePoints = points;

            InitializeComponents();
            SetupLineRenderer();
            SetupCollider();
            CalculateLineLength();

            UsageCount = 0;
            Lifetime = 0f;

            _isInitialized = true;

            AppLogger.Debug($"[FreeformTrampoline] Initialized with {points.Length} points");
        }

        /// <summary>
        /// IPoolable.Release Implementation
        /// </summary>
        public override void Release()
        {
            _linePoints = null;

            base.Release();
        }

        /// <summary>
        /// IPoolable.Destroy Implementation
        /// </summary>
        public override void Destroy()
        {
            _linePoints = null;

            base.Destroy();
        }
        #endregion


        #region Protected Methods
        /// <summary>
        /// LineRenderer를 설정합니다.
        /// </summary>
        protected override void SetupLineRenderer()
        {
            _lineRenderer.positionCount = _linePoints.Length;
            _lineRenderer.startWidth = _config.LineWidth;
            _lineRenderer.endWidth = _config.LineWidth;
            _lineRenderer.material = _config.LineMaterial;
            _lineRenderer.startColor = _config.LineColor;
            _lineRenderer.endColor = _config.LineColor;

            for (int i = 0; i < _linePoints.Length; i++)
            {
                _lineRenderer.SetPosition(i, _linePoints[i]);
            }
        }

        /// <summary>
        /// EdgeCollider2D를 설정합니다.
        /// </summary>
        protected override void SetupCollider()
        {
            // EdgeCollider2D는 Local Space를 사용하므로 World Space -> Local Space 좌표 변환 수행
            Vector2[] localPoints = new Vector2[_linePoints.Length];
            for (int i = 0; i < _linePoints.Length; i++)
            {
                localPoints[i] = transform.InverseTransformPoint(_linePoints[i]);
            }

            _edgeCollider.points = localPoints;
        }

        /// <summary>
        /// 라인의 특정 지점에서의 접촉 정보를 계산합니다.
        /// </summary>
        protected override ContactInfo AnalyzeContactInfo(Vector2 contactPoint)
        {
            float minDistance = float.MaxValue;
            int closestSegmentIndex = 0;

            // point로부터 가장 가까운 선분 확인 (접촉 지점)
            float closestProjectionLength = 0f;
            for (int i = 0; i < _linePoints.Length - 1; i++)
            {
                float distance = GetDistanceToSegment(contactPoint, _linePoints[i], _linePoints[i + 1], out float projectionLength);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestSegmentIndex = i;
                    closestProjectionLength = projectionLength;
                }
            }

            // 시작점부터 현재 segment까지의 누적 거리 계산
            float distanceFromStart = 0f;
            for (int j = 0; j < closestSegmentIndex; j++)
            {
                distanceFromStart += Vector2.Distance(_linePoints[j], _linePoints[j + 1]);
            }
            distanceFromStart += closestProjectionLength;

            // 노말 벡터 계산
            Vector2 segmentDirection = (_linePoints[closestSegmentIndex + 1] - _linePoints[closestSegmentIndex]).normalized;
            Vector2 normal = new Vector2(-segmentDirection.y, segmentDirection.x); // 90도 반시계방향 회전

            // 노말 벡터는 항상 Upward를 향해야 함
            if (normal.y < 0f)
            {
                normal = -normal;
            }

            return new ContactInfo
            {
                ContactPoint = contactPoint,
                ClosestSegmentIndex = closestSegmentIndex,
                DistanceFromStart = distanceFromStart,
                Normal = normal.normalized
            };
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 라인의 총 길이를 계산합니다. (중앙 계산 등에 사용)
        /// </summary>
        private void CalculateLineLength()
        {
            _lineLength = 0f;
            for (int i = 0; i < _linePoints.Length - 1; i++)
            {
                _lineLength += Vector2.Distance(_linePoints[i], _linePoints[i + 1]);
            }
        }

        /// <summary>
        /// 점에서 선분까지의 최단 거리를 계산합니다.
        /// </summary>
        private float GetDistanceToSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd, out float projectionLength)
        {
            Vector2 segmentVector = segmentEnd - segmentStart;
            Vector2 pointVector = point - segmentStart;

            float segmentLength = segmentVector.magnitude;
            Vector2 segmentDirection = segmentVector / segmentLength; // projection 계산에 사용하기 위해 segmentLength를 이미 구했기 때문에 normalized 사용하지 않음.

            // Projection 수행
            projectionLength = Vector2.Dot(pointVector, segmentDirection);
            projectionLength = Mathf.Clamp(projectionLength, 0f, segmentLength);

            Vector2 closestPoint = segmentStart + segmentDirection * projectionLength;

            return Vector2.Distance(point, closestPoint);
        }
        #endregion
    }
}