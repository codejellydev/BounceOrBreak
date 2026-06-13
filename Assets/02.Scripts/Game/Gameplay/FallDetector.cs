using CodeJelly.BounceOrBreak.Core.Logging;
using CodeJelly.BounceOrBreak.Game.Character;
using R3;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay
{
    /// <summary>
    /// 낙하한 ICharacter를 감지하는 클래스.
    /// 감지 사실만 발행하며, 게임오버 여부 판단은 구독 클래스가 처리합니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FallDetector : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// ICharacter가 낙하 경계에 진입했을 때 발행
        /// </summary>
        public Observable<ICharacter> OnCharacterFell => _onCharacterFell;
        #endregion


        #region Private Variables
        private readonly Subject<ICharacter> _onCharacterFell = new Subject<ICharacter>();

        private Collider2D _collider;
        #endregion


        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<ICharacter>(out var character) == false)
            {
                return;
            }

            AppLogger.Info($"[FallDetector] ICharacter fell: {other.name}.");
            _onCharacterFell.OnNext(character);
        }

        private void OnDestroy()
        {
            _onCharacterFell.Dispose();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 필요한 컴포넌트들을 초기화합니다.
        /// </summary>
        private void InitializeComponents()
        {
            _collider ??= GetComponent<Collider2D>();

            // non-trigger면 충돌체를 물리적으로 막아 통과 감지가 불가능하므로 강제 trigger 설정
            _collider.isTrigger = true;
        }
        #endregion
    }
}