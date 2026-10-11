using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Settings.Graphics
{
    /// <summary>
    /// 품질 수준별 그래픽 프리셋을 담는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "GraphicsPresetConfig", menuName = "Configs/Settings/Graphics Preset Config")]
    public class GraphicsPresetConfig : ScriptableObject
    {
        #region Nested Classes
        /// <summary>
        /// 품질 수준 하나의 프리셋.
        /// </summary>
        [System.Serializable]
        public class Preset
        {
            #region Properties
            public int TargetFrameRate => _targetFrameRate;
            #endregion


            #region Private Variables
            [Tooltip("목표 프레임레이트. 0이면 기기의 화면 주사율을 따름")]
            [SerializeField] private int _targetFrameRate = GameConstants.Graphics.DEVICE_MAX_FRAME_RATE;
            #endregion
        }
        #endregion


        #region Properties
        /// <summary>
        /// 품질 수준 인덱스로 찾는 프리셋 배열.
        /// </summary>
        public Preset[] Presets => _presets;
        #endregion


        #region Private Variables
        [SerializeField] private Preset[] _presets;
        #endregion


        #region Public Methods
        /// <summary>
        /// 품질 수준에 해당하는 프리셋을 찾습니다.
        /// </summary>
        public bool TryGetPreset(int qualityLevel, out Preset preset)
        {
            if (_presets == null || qualityLevel < 0 || qualityLevel >= _presets.Length)
            {
                preset = null;

                return false;
            }

            preset = _presets[qualityLevel];

            return preset != null;
        }
        #endregion
    }
}