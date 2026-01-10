using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Character
{
    /// <summary>
    /// Character의 물리 로직을 담당하는 클래스.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public class CharacterPhysics : MonoBehaviour
    {
        #region Private Variables
        private CircleCollider2D _collider;
        private Rigidbody2D _rigidbody;
        #endregion
    }
}
