namespace NotificationMonitor.Core.EventArgs
{
    /// <summary>
    /// 状态变更事件参数类
    /// 用于传递监听器状态变更信息
    /// </summary>
    public class StatusChangedEventArgs : System.EventArgs
    {
        #region 嵌套类型

        /// <summary>
        /// 监听器状态枚举
        /// </summary>
        public enum ListenerStatus : int
        {
            /// <summary>
            /// 已停止
            /// </summary>
            Stopped,

            /// <summary>
            /// 正在启动
            /// </summary>
            Starting,

            /// <summary>
            /// 正在运行
            /// </summary>
            Running,

            /// <summary>
            /// 正在停止
            /// </summary>
            Stopping,

            /// <summary>
            /// 权限被拒绝
            /// </summary>
            PermissionDenied,

            /// <summary>
            /// 发生错误
            /// </summary>
            Error
        }

        #endregion

        #region public 属性

        /// <summary>
        /// 获取或设置当前状态
        /// </summary>
        public ListenerStatus Status { get; set; }

        /// <summary>
        /// 获取或设置状态描述消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        #endregion
    }
}
