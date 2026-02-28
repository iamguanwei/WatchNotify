using NotificationMonitor.Core.Entity;

namespace NotificationMonitor.Core.EventArgs
{
    /// <summary>
    /// 通知事件参数类
    /// 用于传递接收到的通知信息
    /// </summary>
    public class NotificationEventArgs : System.EventArgs
    {
        #region public 属性

        /// <summary>
        /// 获取或设置通知信息
        /// </summary>
        public NotificationInfo Notification { get; set; } = new NotificationInfo();

        #endregion
    }
}
