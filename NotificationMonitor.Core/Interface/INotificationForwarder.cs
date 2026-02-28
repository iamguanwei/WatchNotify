using System.Threading;
using System.Threading.Tasks;
using NotificationMonitor.Core.Entity;
using NotificationMonitor.Core.EventArgs;

namespace NotificationMonitor.Core.Interface
{
    /// <summary>
    /// 通知转发器接口
    /// 定义通知转发的标准行为
    /// </summary>
    public interface INotificationForwarder
    {
        /// <summary>
        /// 转发通知到目标服务
        /// </summary>
        /// <param name="notification">通知信息</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>转发结果</returns>
        Task<ForwardResult> ForwardAsync(NotificationInfo notification, CancellationToken cancellationToken = default);
    }
}
