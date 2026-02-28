using System;
using NotificationMonitor.Core.Entity;
using NotificationMonitor.Core.EventArgs;
using NotificationMonitor.Core.Interface;

namespace NotificationMonitor.Core.Common
{
    /// <summary>
    /// 事件输出接收器
    /// 通过事件回调将输出信息传递给订阅者
    /// 适用于UI程序获取输出内容的场景
    /// </summary>
    public class EventOutputSink : IOutputSink
    {
        #region public 事件

        /// <summary>
        /// 输出接收事件，当有新的输出时触发
        /// </summary>
        public event EventHandler<OutputEventArgs>? OutputReceived;

        #endregion

        #region IOutputSink 实现

        /// <summary>
        /// 触发输出事件
        /// </summary>
        /// <param name="level">输出级别</param>
        /// <param name="category">输出分类</param>
        /// <param name="message">消息内容</param>
        public void Write(OutputLevel level, string category, string message)
        {
            OutputReceived?.Invoke(this, new OutputEventArgs
            {
                Level = level,
                Category = category,
                Message = message,
                Timestamp = DateTime.Now
            });
        }

        #endregion
    }
}
