using CodeJelly.BounceOrBreak.Core.Pooling;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Trampoline
{
    /// <summary>
    /// TrampolineDrawer에 의해 그려지는 Trampoline 오브젝트의 공통 인터페이스.
    /// IBounceable 객체와 접촉 시 튕김 처리를 수행합니다.
    /// </summary>
    public interface ITrampoline : IPoolable
    {
        /// <summary>
        /// Trampoline의 타입
        /// </summary>
        TrampolineType Type { get; }

        /// <summary>
        /// 현재 사용된 횟수
        /// </summary>
        int UsageCount { get; }

        /// <summary>
        /// 생성 후 경과 시간
        /// </summary>
        float Lifetime { get; }

        /// <summary>
        /// Trampoline을 초기화합니다.
        /// </summary>
        /// <returns>초기화 성공 시 true</returns>
        bool Initialize(TrampolineDrawer drawer, TrampolineConfig config, Vector2[] points);
    }
}