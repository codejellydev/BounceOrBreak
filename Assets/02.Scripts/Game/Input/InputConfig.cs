using CodeJelly.BounceOrBreak.Core.Logging;
using System.Collections.Generic;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Input
{
    /// <summary>
    /// Context별 Input Action Map 설정을 관리하는 ScriptableObject.
    /// TODO: 빌드 프로세스에서 모든 InputContext에 대응하는 Profile 존재 여부 검증
    /// </summary>
    [CreateAssetMenu(fileName = "InputConfig", menuName = "Configs/Game/Input Config")]
    public class InputConfig : ScriptableObject
    {
        #region Nested Classes
        /// <summary>
        /// 특정 Input Context에서 활성화할 Input Action Map 설정.
        /// </summary>
        [System.Serializable]
        public class InputProfile
        {
            [Header("Input Context")]
            public InputContext Context;

            [Header("Input Maps")]
            [Tooltip("활성화할 Input Action Map들의 조합")]
            public InputMapType ActiveMapFlags;
        }
        #endregion


        #region Properties
        /// <summary>
        /// 모든 Context별 Input Profile 배열.
        /// </summary>
        public InputProfile[] Profiles => _profiles;
        #endregion


        #region Private Variables
        /// <summary>
        /// Context별 기본 Input Map 설정.
        /// Reset() 시 이 값들로 초기화됩니다.
        /// </summary>
        private readonly Dictionary<InputContext, InputMapType> _defaultMaps
            = new Dictionary<InputContext, InputMapType>()
        {
            { InputContext.None, InputMapType.None },
            { InputContext.Pause, InputMapType.UI | InputMapType.System },
            { InputContext.Loading, InputMapType.UI | InputMapType.System },
            { InputContext.Lobby, InputMapType.UI | InputMapType.System },
            { InputContext.Gameplay, InputMapType.Player | InputMapType.UI | InputMapType.System },
            { InputContext.GameOver, InputMapType.UI | InputMapType.System },
        };

        private InputProfile[] _profiles;
        #endregion


        #region Unity Lifecycle
        /// <summary>
        /// ScriptableObject 생성 시 기본 프로필을 초기화합니다.
        /// InputContext enum 수정 시 Inspector에서 Reset 메뉴를 실행하여 프로필 재생성이 필요합니다.
        /// </summary>
        private void Reset()
        {
            InitializeProfiles();
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// InputContext enum의 모든 값에 대해 기본 프로필을 생성합니다.
        /// Fallback InputMapType은 None으로 설정됩니다.
        /// </summary>
        private void InitializeProfiles()
        {
            var contexts = System.Enum.GetValues(typeof(InputContext));
            _profiles = new InputProfile[contexts.Length];

            for (int i = 0; i < contexts.Length; i++)
            {
                InputContext context = (InputContext)contexts.GetValue(i);

                InputMapType enabledMaps;
                if (_defaultMaps.TryGetValue(context, out enabledMaps) == false)
                {
                    enabledMaps = InputMapType.None;
                    AppLogger.Warning($"[InputConfig] No default map defined for {context}. Set to None.");
                }

                _profiles[i] = new InputProfile
                {
                    Context = context,
                    ActiveMapFlags = enabledMaps
                };
            }
        }
        #endregion
    }
}