namespace CodeJelly.BounceOrBreak.Game.UI.Message
{
    /// <summary>
    /// MessagePanel에 표시할 내용 데이터.
    /// </summary>
    public struct MessageRequest
    {
        #region Properties
        /// <summary>
        /// 제목.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 본문.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 주 버튼 라벨.
        /// </summary>
        public string PrimaryLabel { get; set; }

        /// <summary>
        /// 보조 버튼 라벨.
        /// </summary>
        public string SecondaryLabel { get; set; }

        /// <summary>
        /// 주 버튼이 되돌릴 수 없는 동작인지 여부.
        /// </summary>
        public bool IsDestructive { get; set; }
        #endregion
    }
}