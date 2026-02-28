using NotificationMonitor.Core.Entity;

namespace NotificationMonitor.Core.Interface
{
    /// <summary>
    /// 输出接收器接口
    /// 用于接收程序运行时的输出信息，实现输出与UI解耦
    /// </summary>
    public interface IOutputSink
    {
        /// <summary>
        /// 输出信息
        /// </summary>
        /// <param name="level">输出级别</param>
        /// <param name="category">输出分类（如：通知、转发、错误等）</param>
        /// <param name="message">消息内容</param>
        void Write(OutputLevel level, string category, string message);
    }
}
