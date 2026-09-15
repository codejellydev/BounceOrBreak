using CodeJelly.BounceOrBreak.Core.Logging;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// 시작점과 끝점을 직선으로 연결한 일자형 Trampoline.
    /// </summary>
    public class StraightTrampoline : TrampolineBase
    {
        #region Properties
        /// <summary>
        /// ITrampoline.Type Implementation
        /// </summary>
        public override TrampolineType Type => TrampolineType.Straight;
        #endregion


        #region Private Variables
        private Vector2 _startPoint;
        private Vector2 _endPoint;
        private Vector2 _lineDirection;
        private Vector2 _normal;
        #endregion


        #region Public Methods
        /// <summary>
        /// ITrampoline.Initialize Implementation
        /// </summary>
        /// <param name="drawer">Trampoline을 생성한 Drawer</param>
        /// <param name="config">Trampoline 설정</param>
        /// <param name="points">LineRenderer의 point 배열 (시작점, 끝점 2개)</param>
        /// <returns>초기화 성공 시 true</returns>
        public override bool Initialize(TrampolineDrawer drawer, TrampolineConfig config, Vector2[] points)
        {
            if (points.Length != 2)
            {
                AppLogger.Error($"[StraightTrampoline] Invalid points count: {points.Length}. Expected 2.");

                return false;
            }

            _drawer = drawer;
            _config = config;
            _startPoint = points[0];
            _endPoint = points[1];

            InitializeComponents();
            CalculateLineProperties();
            ApplyVisual(points);
            SetupCollider();
            SetupPlatformEffector();

            Spawn();

            AppLogger.Debug("[StraightTrampoline] Initialized.");

            return true;
        }
        #endregion


        #region Protected Methods
        /// <summary>
        /// EdgeCollider2D를 설정합니다.
        /// </summary>
        protected override void SetupCollider()
        {
            // EdgeCollider2D는 Local Space를 사용하므로 World Space -> Local Space 좌표 변환 수행
            Vector2[] localPoints = new Vector2[2];
            localPoints[0] = transform.InverseTransformPoint(_startPoint);
            localPoints[1] = transform.InverseTransformPoint(_endPoint);

            _edgeCollider.points = localPoints;

            // 판정면을 보이는 윗면에 맞춤. 0이면 두께 없는 중심선에서 튕겨 계란이 몸통에 박힌 것처럼 보임
            _edgeCollider.edgeRadius = _config.Thickness * 0.5f;
        }

        /// <summary>
        /// 라인의 특정 지점에서의 접촉 정보를 계산합니다.
        /// </summary>
        protected override ContactInfo AnalyzeContactInfo(Vector2 contactPoint)
        {
            float distanceFromStart = Vector2.Distance(_startPoint, contactPoint);

            return new ContactInfo
            {
                ContactPoint = contactPoint,
                Normal = _normal,
                DistanceFromStart = distanceFromStart,
                ClosestSegmentIndex = 0  // 선분이 하나뿐이므로 항상 0
            };
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 라인의 속성들을 미리 계산합니다.
        /// </summary>
        private void CalculateLineProperties()
        {
            _lineLength = Vector2.Distance(_startPoint, _endPoint);
            _lineDirection = (_endPoint - _startPoint).normalized;
            _normal = new Vector2(-_lineDirection.y, _lineDirection.x); // 90도 반시계방향 회전

            // 노말 벡터는 항상 Upward를 향해야 함
            if (_normal.y < 0f)
            {
                _normal = -_normal;
            }
        }
        #endregion
    }
}