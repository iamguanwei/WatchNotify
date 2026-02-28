using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NotificationMonitor.Core.Entity;
using NotificationMonitor.Core.EventArgs;
using NotificationMonitor.Core.Interface;

namespace NotificationMonitor
{
    /// <summary>
    /// HTTP通知转发器
    /// 将通知转发到指定的HTTP/HTTPS地址，支持频率限制和排队等待
    /// </summary>
    public class HttpForwarder : INotificationForwarder, IDisposable
    {
        #region 常量

        /// <summary>
        /// 转发间隔时间（毫秒），每10秒允许转发一次
        /// </summary>
        private const int FORWARD_INTERVAL_MS = 10000;

        #endregion

        #region private 字段

        private readonly HttpClient _httpClient;
        private readonly string _forwardUrl;
        private DateTime _lastForwardTime;
        private readonly ConcurrentQueue<PendingNotification> _pendingQueue;
        private readonly object _forwardLock;
        private bool _isProcessing;
        private bool _isDisposed;

        #endregion

        #region public 属性

        /// <summary>
        /// 获取当前使用的转发URL
        /// </summary>
        public string ForwardUrl => _forwardUrl;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化HTTP转发器实例
        /// </summary>
        /// <param name="forwardUrl">转发URL，支持{0}占位符替换消息内容</param>
        public HttpForwarder(string forwardUrl)
        {
            _forwardUrl = forwardUrl ?? string.Empty;
            _lastForwardTime = DateTime.MinValue;
            _httpClient = new HttpClient();
            _pendingQueue = new ConcurrentQueue<PendingNotification>();
            _forwardLock = new object();
            _isProcessing = false;
            _isDisposed = false;
        }

        #endregion

        #region INotificationForwarder 实现

        /// <summary>
        /// 转发通知到指定URL
        /// 如果距离上次转发不足10秒，通知将加入队列等待发送
        /// </summary>
        /// <param name="notification">通知信息</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>转发结果</returns>
        public async Task<ForwardResult> ForwardAsync(NotificationInfo notification, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
            {
                return ForwardResult.Failure("转发器已释放");
            }

            if (string.IsNullOrEmpty(_forwardUrl))
            {
                return ForwardResult.Failure("转发URL未配置");
            }

            try
            {
                string message = notification.ToString();
                var waitTime = calculateWaitTime();

                if (waitTime > TimeSpan.Zero)
                {
                    var pending = new PendingNotification(notification, new TaskCompletionSource<ForwardResult>());
                    _pendingQueue.Enqueue(pending);
                    processQueueAsync(cancellationToken);
                    return await pending.TaskCompletionSource.Task;
                }

                return await sendNotificationAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                return ForwardResult.Failure($"转发通知时发生错误: {ex.Message}", ex);
            }
        }

        #endregion

        #region private 方法

        /// <summary>
        /// 计算需要等待的时间
        /// </summary>
        /// <returns>需要等待的时间间隔，无需等待返回Zero</returns>
        private TimeSpan calculateWaitTime()
        {
            lock (_forwardLock)
            {
                var elapsed = DateTime.Now - _lastForwardTime;
                if (elapsed >= TimeSpan.FromMilliseconds(FORWARD_INTERVAL_MS))
                {
                    return TimeSpan.Zero;
                }
                return TimeSpan.FromMilliseconds(FORWARD_INTERVAL_MS) - elapsed;
            }
        }

        /// <summary>
        /// 异步处理队列中的待发送通知
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        private async void processQueueAsync(CancellationToken cancellationToken)
        {
            lock (_forwardLock)
            {
                if (_isProcessing)
                {
                    return;
                }
                _isProcessing = true;
            }

            try
            {
                while (!_isDisposed && _pendingQueue.TryDequeue(out var pending))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        pending.TaskCompletionSource.TrySetCanceled();
                        continue;
                    }

                    var waitTime = calculateWaitTime();
                    if (waitTime > TimeSpan.Zero)
                    {
                        await Task.Delay(waitTime, cancellationToken);
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        pending.TaskCompletionSource.TrySetCanceled();
                        continue;
                    }

                    var result = await sendNotificationAsync(pending.Notification.ToString(), cancellationToken);
                    pending.TaskCompletionSource.TrySetResult(result);
                }
            }
            catch (OperationCanceledException)
            {
                while (_pendingQueue.TryDequeue(out var pending))
                {
                    pending.TaskCompletionSource.TrySetCanceled();
                }
            }
            finally
            {
                lock (_forwardLock)
                {
                    _isProcessing = false;
                }
            }
        }

        /// <summary>
        /// 发送通知到指定URL
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>转发结果</returns>
        private async Task<ForwardResult> sendNotificationAsync(string message, CancellationToken cancellationToken)
        {
            try
            {
                string url = _forwardUrl.Contains("{0}")
                    ? string.Format(_forwardUrl, Uri.EscapeDataString(message))
                    : _forwardUrl;

                var content = new StringContent(message, Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync(url, content, cancellationToken);

                lock (_forwardLock)
                {
                    _lastForwardTime = DateTime.Now;
                }

                if (response.IsSuccessStatusCode)
                {
                    return ForwardResult.Success($"通知已成功发送: {message}");
                }
                else
                {
                    return ForwardResult.Failure($"发送失败，HTTP状态码: {(int)response.StatusCode} {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return ForwardResult.Failure($"转发通知时发生错误: {ex.Message}", ex);
            }
        }

        #endregion

        #region IDisposable 实现

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _httpClient.Dispose();

            while (_pendingQueue.TryDequeue(out var pending))
            {
                pending.TaskCompletionSource.TrySetCanceled();
            }
        }

        #endregion

        #region 嵌套类型

        /// <summary>
        /// 待发送通知项
        /// </summary>
        private class PendingNotification
        {
            #region public 属性

            /// <summary>
            /// 获取通知信息
            /// </summary>
            public NotificationInfo Notification { get; }

            /// <summary>
            /// 获取任务完成源
            /// </summary>
            public TaskCompletionSource<ForwardResult> TaskCompletionSource { get; }

            #endregion

            #region 构造函数

            /// <summary>
            /// 初始化待发送通知项
            /// </summary>
            /// <param name="notification">通知信息</param>
            /// <param name="taskCompletionSource">任务完成源</param>
            public PendingNotification(NotificationInfo notification, TaskCompletionSource<ForwardResult> taskCompletionSource)
            {
                Notification = notification;
                TaskCompletionSource = taskCompletionSource;
            }

            #endregion
        }

        #endregion
    }
}
