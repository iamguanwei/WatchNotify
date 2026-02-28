using NotificationMonitor.Core.Entity;

namespace NotificationMonitor.Core.EventArgs
{
    /// <summary>
    /// 通知处理完成事件参数类
    /// 用于传递通知处理的完整结果
    /// </summary>
    public class NotificationProcessedEventArgs : System.EventArgs
    {
        #region public 属性

        /// <summary>
        /// 获取或设置通知信息
        /// </summary>
        public NotificationInfo Notification { get; set; } = new NotificationInfo();

        /// <summary>
        /// 获取或设置转发结果
        /// </summary>
        public ForwardResult? ForwardResult { get; set; }

        /// <summary>
        /// 获取或设置处理是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        #endregion
    }
}
