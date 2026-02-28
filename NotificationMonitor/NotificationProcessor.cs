using System;
using System.Threading.Tasks;
using NotificationMonitor.Core.Entity;
using NotificationMonitor.Core.EventArgs;
using NotificationMonitor.Core.Interface;

namespace NotificationMonitor
{
    /// <summary>
    /// 通知处理器
    /// 协调通知监听器和转发器的工作流程
    /// </summary>
    public class NotificationProcessor : IDisposable
    {
        #region public 事件

        /// <summary>
        /// 通知处理完成事件
        /// </summary>
        public event EventHandler<NotificationProcessedEventArgs>? NotificationProcessed;

        #endregion

        #region private 字段

        private readonly NotificationListener _listener;
        private readonly INotificationForwarder _forwarder;
        private readonly IOutputSink _outputSink;
        private bool _isRunning;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化通知处理器实例
        /// </summary>
        /// <param name="listener">通知监听器</param>
        /// <param name="forwarder">通知转发器</param>
        /// <param name="outputSink">输出接收器</param>
        public NotificationProcessor(
            NotificationListener listener,
            INotificationForwarder forwarder,
            IOutputSink outputSink)
        {
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
            _forwarder = forwarder ?? throw new ArgumentNullException(nameof(forwarder));
            _outputSink = outputSink ?? throw new ArgumentNullException(nameof(outputSink));
            _isRunning = false;
        }

        #endregion

        #region public 方法

        /// <summary>
        /// 启动通知处理器
        /// </summary>
        /// <returns>启动是否成功</returns>
        public async Task<bool> StartAsync()
        {
            if (_isRunning)
            {
                _outputSink.Write(OutputLevel.Warning, "处理器", "通知处理器已在运行中");
                return true;
            }

            _outputSink.Write(OutputLevel.Info, "处理器", "正在启动通知处理器...");

            _listener.NotificationReceived += handleNotificationReceived;
            _listener.StatusChanged += handleStatusChanged;

            bool started = await _listener.StartAsync();

            if (started)
            {
                _isRunning = true;
                _outputSink.Write(OutputLevel.Info, "处理器", "通知处理器启动成功");
            }
            else
            {
                _outputSink.Write(OutputLevel.Error, "处理器", "通知处理器启动失败");
            }

            return started;
        }

        /// <summary>
        /// 停止通知处理器
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            _outputSink.Write(OutputLevel.Info, "处理器", "正在停止通知处理器...");

            _listener.NotificationReceived -= handleNotificationReceived;
            _listener.StatusChanged -= handleStatusChanged;
            _listener.Stop();

            _isRunning = false;
            _outputSink.Write(OutputLevel.Info, "处理器", "通知处理器已停止");
        }

        #endregion

        #region private 方法 - 事件处理

        /// <summary>
        /// 处理通知接收事件
        /// </summary>
        private async void handleNotificationReceived(object? sender, NotificationEventArgs e)
        {
            var notification = e.Notification;

            _outputSink.Write(OutputLevel.Info, "通知", $"发送者: {notification.Sender}");
            _outputSink.Write(OutputLevel.Info, "通知", $"时间: {notification.Time}");
            _outputSink.Write(OutputLevel.Info, "通知", $"标题: {notification.Title}");
            _outputSink.Write(OutputLevel.Info, "通知", $"内容: {notification.Content}");

            ForwardResult? forwardResult = null;
            bool isSuccess = true;

            try
            {
                forwardResult = await _forwarder.ForwardAsync(notification);

                if (forwardResult.IsSuccess)
                {
                    _outputSink.Write(OutputLevel.Info, "转发", forwardResult.Message);
                }
                else
                {
                    _outputSink.Write(OutputLevel.Warning, "转发", forwardResult.Message);
                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _outputSink.Write(OutputLevel.Error, "转发", $"转发通知时发生错误: {ex.Message}");
                forwardResult = ForwardResult.Failure(ex.Message, ex);
                isSuccess = false;
            }

            onNotificationProcessed(notification, forwardResult, isSuccess);
        }

        /// <summary>
        /// 处理状态变更事件
        /// </summary>
        private void handleStatusChanged(object? sender, StatusChangedEventArgs e)
        {
            var level = e.Status switch
            {
                StatusChangedEventArgs.ListenerStatus.Running => OutputLevel.Info,
                StatusChangedEventArgs.ListenerStatus.Stopped => OutputLevel.Info,
                StatusChangedEventArgs.ListenerStatus.PermissionDenied => OutputLevel.Error,
                StatusChangedEventArgs.ListenerStatus.Error => OutputLevel.Error,
                _ => OutputLevel.Info
            };

            _outputSink.Write(level, "状态", e.Message);
        }

        #endregion

        #region private 方法 - 事件触发

        /// <summary>
        /// 触发通知处理完成事件
        /// </summary>
        private void onNotificationProcessed(NotificationInfo notification, ForwardResult? forwardResult, bool isSuccess)
        {
            NotificationProcessed?.Invoke(this, new NotificationProcessedEventArgs
            {
                Notification = notification,
                ForwardResult = forwardResult,
                IsSuccess = isSuccess
            });
        }

        #endregion

        #region IDisposable 实现

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}
