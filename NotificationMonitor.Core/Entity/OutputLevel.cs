namespace NotificationMonitor.Core.Entity
{
    /// <summary>
    /// 输出级别枚举
    /// </summary>
    public enum OutputLevel : int
    {
        /// <summary>
        /// 普通信息
        /// </summary>
        Info,

        /// <summary>
        /// 警告信息
        /// </summary>
        Warning,

        /// <summary>
        /// 错误信息
        /// </summary>
        Error,

        /// <summary>
        /// 调试信息
        /// </summary>
        Debug
    }
}
