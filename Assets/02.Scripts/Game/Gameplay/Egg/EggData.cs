using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Gameplay.Egg
{
    /// <summary>
    /// Egg 개체의 데이터를 정의하는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "EggData", menuName = "Configs/Game/Egg Data")]
    public class EggData : ScriptableObject
    {
        #region Properties
        public EggType Type => _type;

        public EggColor Color => _color;

        public Sprite BaseSprite => _baseSprite;
        #endregion


        #region Private Variables
        [Tooltip("EggRegistry 조회에 사용되는 EggType")]
        [SerializeField] private EggType _type;

        [Tooltip("Goal 매칭에 사용되는 EggColor")]
        [SerializeField] private EggColor _color;

        [Tooltip("기본 스프라이트")]
        [SerializeField] private Sprite _baseSprite;
        #endregion
    }
}
