using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// Egg 타입별 데이터를 정의하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "EggTypeData", menuName = "Configs/Game/Egg Type Data")]
    public class EggTypeData : ScriptableObject
    {
        #region Properties
        public EggType Type => _type;

        public Color Color => _color;
        #endregion


        #region Private Variables
        [Tooltip("계란 타입")]
        [SerializeField] private EggType _type;

        [Tooltip("계란 색")]
        [SerializeField] private Color _color = Color.white;
        #endregion
    }
}