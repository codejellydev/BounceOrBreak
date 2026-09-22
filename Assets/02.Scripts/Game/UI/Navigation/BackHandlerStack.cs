using System;
using System.Collections.Generic;

namespace CodeJelly.BounceOrBreak.Game.UI.Navigation
{
    /// <summary>
    /// 뒤로가기 핸들러 스택.
    /// 맨 위 핸들러 하나만 호출하며, 해제는 순서와 무관하게 자기 항목만 제거합니다.
    /// </summary>
    public class BackHandlerStack
    {
        #region Nested Classes
        /// <summary>
        /// 스택 항목. Dispose하면 스택에서 빠집니다.
        /// </summary>
        private sealed class Entry : IDisposable
        {
            #region Constructors
            public Entry(List<Entry> owner, Action handler)
            {
                _owner = owner;
                Handler = handler;
            }
            #endregion


            #region Properties
            public Action Handler { get; }
            #endregion


            #region Private Variables
            private readonly List<Entry> _owner;
            #endregion


            #region Public Methods
            /// <summary>
            /// IDisposable.Dispose Implementation
            /// </summary>
            public void Dispose()
            {
                _owner.Remove(this);
            }
            #endregion
        }
        #endregion


        #region Properties
        /// <summary>
        /// 쌓여 있는 핸들러 개수
        /// </summary>
        public int Count => _entries.Count;
        #endregion


        #region Private Variables
        private readonly List<Entry> _entries = new List<Entry>();
        #endregion


        #region Public Methods
        /// <summary>
        /// 핸들러를 맨 위에 올립니다.
        /// </summary>
        /// <returns>해제용 IDisposable. Dispose하면 스택에서 빠짐</returns>
        public IDisposable Push(Action handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Entry entry = new Entry(_entries, handler);
            _entries.Add(entry);

            return entry;
        }

        /// <summary>
        /// 맨 위 핸들러를 호출합니다.
        /// </summary>
        /// <returns>핸들러가 있었으면 true</returns>
        public bool Handle()
        {
            if (_entries.Count == 0)
            {
                return false;
            }

            _entries[^1].Handler();

            return true;
        }
        #endregion
    }
}