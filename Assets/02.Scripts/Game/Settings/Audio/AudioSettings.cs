using System;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Settings.Audio
{
    /// <summary>
    /// 오디오 설정 데이터 모델.
    /// </summary>
    [Serializable]
    public class AudioSettings
    {
        #region Properties
        /// <summary>
        /// 마스터 볼륨 (0.0 ~ 1.0).
        /// </summary>
        public float MasterVolume
        {
            get => _masterVolume;
            set => _masterVolume = Mathf.Clamp01(value);
        }

        /// <summary>
        /// 배경음악 볼륨 (0.0 ~ 1.0).
        /// </summary>
        public float BgmVolume
        {
            get => _bgmVolume;
            set => _bgmVolume = Mathf.Clamp01(value);
        }

        /// <summary>
        /// 효과음 볼륨 (0.0 ~ 1.0).
        /// </summary>
        public float SfxVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = Mathf.Clamp01(value);
        }
        #endregion


        #region Private Variables
        [SerializeField] private float _masterVolume = GameConstants.Audio.Volume.DEFAULT;
        [SerializeField] private float _bgmVolume = GameConstants.Audio.Volume.DEFAULT;
        [SerializeField] private float _sfxVolume = GameConstants.Audio.Volume.DEFAULT;
        #endregion
    }
}