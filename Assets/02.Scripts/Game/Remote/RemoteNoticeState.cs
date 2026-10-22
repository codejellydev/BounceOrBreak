using System;
using UnityEngine;

namespace CodeJelly.BounceOrBreak.Game.Remote
{
    /// <summary>
    /// 원격 안내의 열람 기록 데이터 모델.
    /// </summary>
    [Serializable]
    public class RemoteNoticeState
    {
        #region Properties
        /// <summary>
        /// 새 버전 안내를 넘긴 버전. 같은 버전은 다시 띄우지 않습니다.
        /// </summary>
        public string DismissedVersion
        {
            get => _dismissedVersion;
            set => _dismissedVersion = value;
        }

        /// <summary>
        /// 마지막으로 본 공지의 식별자.
        /// </summary>
        public string SeenNoticeId
        {
            get => _seenNoticeId;
            set => _seenNoticeId = value;
        }
        #endregion


        #region Private Variables
        [SerializeField] private string _dismissedVersion;
        [SerializeField] private string _seenNoticeId;
        #endregion
    }
}