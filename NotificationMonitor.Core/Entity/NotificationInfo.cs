namespace NotificationMonitor.Core.Entity
{
    /// <summary>
    /// 通知信息实体类
    /// 包含Windows系统通知的详细信息
    /// </summary>
    public class NotificationInfo
    {
        #region public 属性

        /// <summary>
        /// 获取或设置通知的唯一标识符
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// 获取或设置通知发送者名称
        /// </summary>
        public string Sender { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置通知时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 获取或设置通知标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置通知内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        #endregion

        #region public 方法

        /// <summary>
        /// 获取通知的完整文本描述
        /// </summary>
        /// <returns>格式化的通知文本</returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Title))
            {
                return $"[{Sender}] {Time} - {Content}";
            }
            return $"[{Sender}] {Time} - {Title}: {Content}";
        }

        #endregion
    }
}
