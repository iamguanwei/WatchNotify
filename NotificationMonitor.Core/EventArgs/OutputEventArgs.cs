using NotificationMonitor.Core.Entity;

namespace NotificationMonitor.Core.EventArgs
{
    /// <summary>
    /// 输出事件参数类
    /// 用于传递程序运行时的输出信息
    /// </summary>
    public class OutputEventArgs : System.EventArgs
    {
        #region public 属性

        /// <summary>
        /// 获取或设置输出级别
        /// </summary>
        public OutputLevel Level { get; set; }

        /// <summary>
        /// 获取或设置输出分类
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置消息内容
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置输出时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        #endregion
    }
}
