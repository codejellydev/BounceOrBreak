using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.UI.Panel
{
    /// <summary>
    /// UINavigationService가 여는 패널의 Presenter 인터페이스.
    /// 인스턴스 수명은 서비스가 관리하고, 구현체는 열려 있는 동안의 상호작용만 담당합니다.
    /// </summary>
    public interface IUIPanel
    {
        /// <summary>
        /// 패널 키. UIPanelConfig의 Entry와 일치해야 함
        /// </summary>
        string Key { get; }

        /// <summary>
        /// 뒤로가기 등 외부의 취소 요청. 닫기 버튼과 같은 결과로 닫힙니다.
        /// </summary>
        void Cancel();
    }

    /// <summary>
    /// 결과를 반환하는 패널의 Presenter 인터페이스.
    /// </summary>
    public interface IUIPanel<TResult> : IUIPanel
    {
        /// <summary>
        /// 취소 경로로 닫혔을 때의 결과
        /// </summary>
        TResult CancelResult { get; }

        /// <summary>
        /// 인스턴스에 바인딩해 닫힐 때까지 상호작용을 진행하고 결과를 반환합니다.
        /// </summary>
        /// <param name="instance">서비스가 만든 패널 인스턴스</param>
        /// <param name="ct">패널이 사라지면(씬 종료) 취소됨</param>
        UniTask<TResult> RunAsync(GameObject instance, CancellationToken ct);
    }
}