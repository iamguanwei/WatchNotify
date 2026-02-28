using System;
using NotificationMonitor.Core.Entity;
using NotificationMonitor.Core.Interface;

namespace NotificationMonitor.Core.Common
{
    /// <summary>
    /// 控制台输出接收器
    /// 将输出信息直接写入控制台
    /// </summary>
    public class ConsoleOutputSink : IOutputSink
    {
        #region IOutputSink 实现

        /// <summary>
        /// 输出信息到控制台
        /// </summary>
        /// <param name="level">输出级别</param>
        /// <param name="category">输出分类</param>
        /// <param name="message">消息内容</param>
        public void Write(OutputLevel level, string category, string message)
        {
            string prefix = level switch
            {
                OutputLevel.Info => "[信息]",
                OutputLevel.Warning => "[警告]",
                OutputLevel.Error => "[错误]",
                OutputLevel.Debug => "[调试]",
                _ => ""
            };

            Console.WriteLine($"{prefix} [{category}] {message}");
        }

        #endregion
    }
}
