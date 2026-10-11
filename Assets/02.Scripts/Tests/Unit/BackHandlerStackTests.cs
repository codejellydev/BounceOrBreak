using CodeJelly.BounceOrBreak.Game.UI.Navigation;
using NUnit.Framework;
using System;

namespace CodeJelly.BounceOrBreak.Tests.Unit
{
    /// <summary>
    /// BackHandlerStack의 규칙을 검증하는 테스트 클래스.
    /// </summary>
    public class BackHandlerStackTests
    {
        #region Private Variables
        private BackHandlerStack _stack;
        #endregion


        #region Public Methods
        [SetUp]
        public void SetUp()
        {
            _stack = new BackHandlerStack();
        }

        /// <summary>
        /// 핸들러가 없으면 false를 반환합니다.
        /// </summary>
        [Test]
        public void Handle_Empty_ReturnsFalse()
        {
            Assert.IsFalse(_stack.Handle());
        }

        /// <summary>
        /// 여러 핸들러가 있으면 맨 위 하나만 호출합니다.
        /// </summary>
        [Test]
        public void Handle_InvokesTopOnly()
        {
            int bottomCalls = 0;
            int topCalls = 0;

            _stack.Push(() => bottomCalls++);
            _stack.Push(() => topCalls++);

            Assert.IsTrue(_stack.Handle());
            Assert.AreEqual(0, bottomCalls);
            Assert.AreEqual(1, topCalls);
        }

        /// <summary>
        /// 맨 위를 해제하면 그 아래가 맨 위가 됩니다.
        /// </summary>
        [Test]
        public void Dispose_Top_RevealsPrevious()
        {
            int bottomCalls = 0;

            _stack.Push(() => bottomCalls++);
            IDisposable top = _stack.Push(() => { });

            top.Dispose();
            _stack.Handle();

            Assert.AreEqual(1, bottomCalls);
        }

        /// <summary>
        /// 중간을 해제해도 맨 위는 바뀌지 않습니다.
        /// </summary>
        [Test]
        public void Dispose_Middle_KeepsTop()
        {
            int topCalls = 0;

            _stack.Push(() => { });
            IDisposable middle = _stack.Push(() => { });
            _stack.Push(() => topCalls++);

            middle.Dispose();
            _stack.Handle();

            Assert.AreEqual(1, topCalls);
            Assert.AreEqual(2, _stack.Count);
        }

        /// <summary>
        /// 같은 항목을 두 번 해제해도 다른 항목에 영향이 없습니다.
        /// </summary>
        [Test]
        public void Dispose_Twice_IsHarmless()
        {
            _stack.Push(() => { });
            IDisposable top = _stack.Push(() => { });

            top.Dispose();
            top.Dispose();

            Assert.AreEqual(1, _stack.Count);
        }

        /// <summary>
        /// null 핸들러를 올리면 예외가 발생합니다.
        /// </summary>
        [Test]
        public void Push_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _stack.Push(null));
        }
        #endregion
    }
}