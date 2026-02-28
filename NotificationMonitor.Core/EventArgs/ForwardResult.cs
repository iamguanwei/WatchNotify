namespace NotificationMonitor.Core.EventArgs
{
    /// <summary>
    /// 转发结果类
    /// 表示通知转发的执行结果
    /// </summary>
    public class ForwardResult
    {
        #region public 属性

        /// <summary>
        /// 获取或设置转发是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 获取或设置结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置异常信息（如果发生错误）
        /// </summary>
        public Exception? Error { get; set; }

        #endregion

        #region public 静态方法

        /// <summary>
        /// 创建成功的转发结果
        /// </summary>
        /// <param name="message">结果消息</param>
        /// <returns>成功的转发结果实例</returns>
        public static ForwardResult Success(string message = "转发成功")
        {
            return new ForwardResult
            {
                IsSuccess = true,
                Message = message
            };
        }

        /// <summary>
        /// 创建失败的转发结果
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="error">异常对象</param>
        /// <returns>失败的转发结果实例</returns>
        public static ForwardResult Failure(string message, Exception? error = null)
        {
            return new ForwardResult
            {
                IsSuccess = false,
                Message = message,
                Error = error
            };
        }

        #endregion
    }
}
