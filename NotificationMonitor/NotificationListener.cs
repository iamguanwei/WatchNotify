using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using NotificationMonitor.Core.Entity;
using NotificationMonitor.Core.EventArgs;
using NotificationMonitor.Core.Interface;

namespace NotificationMonitor
{
    /// <summary>
    /// 通知监听服务
    /// 使用UserNotificationListener API监测Windows系统通知
    /// </summary>
    public class NotificationListener : IDisposable
    {
        #region 常量

        private const int CACHE_CLEANUP_INTERVAL_MS = 60000;
        private const int MONITOR_INTERVAL_MS = 2000;

        #endregion

        #region public 事件

        /// <summary>
        /// 通知接收事件，当收到新通知时触发
        /// </summary>
        public event EventHandler<NotificationEventArgs>? NotificationReceived;

        /// <summary>
        /// 状态变更事件，当监听器状态发生变化时触发
        /// </summary>
        public event EventHandler<StatusChangedEventArgs>? StatusChanged;

        #endregion

        #region private 字段

        private readonly IOutputSink _outputSink;
        private readonly List<string> _allowedSenderList;
        private readonly HashSet<uint> _notificationIdCache;
        private readonly object _cacheLockObj;

        private UserNotificationListener? _listener;
        private Thread? _monitorThread;
        private bool _isRunning;
        private DateTime _lastCacheCleanupTime;
        private readonly DateTime _startTime;
        private DateTime _lastReceivedNotificationTime;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化通知监听器实例
        /// </summary>
        /// <param name="outputSink">输出接收器</param>
        /// <param name="allowedSenders">允许的发送者列表，为空则接收所有通知</param>
        public NotificationListener(IOutputSink outputSink, IEnumerable<string>? allowedSenders = null)
        {
            _outputSink = outputSink ?? throw new ArgumentNullException(nameof(outputSink));
            _allowedSenderList = allowedSenders?.ToList() ?? new List<string>();
            _notificationIdCache = new HashSet<uint>();
            _cacheLockObj = new object();
            _lastCacheCleanupTime = DateTime.Now;
            _startTime = DateTime.Now;
            _lastReceivedNotificationTime = DateTime.MinValue;
            _isRunning = false;
        }

        #endregion

        #region public 方法

        /// <summary>
        /// 启动监听器
        /// </summary>
        /// <returns>启动是否成功</returns>
        public async Task<bool> StartAsync()
        {
            try
            {
                onStatusChanged(StatusChangedEventArgs.ListenerStatus.Starting, "正在启动通知监听器...");

                _listener = UserNotificationListener.Current;
                var accessStatus = await _listener.RequestAccessAsync();

                if (accessStatus == UserNotificationListenerAccessStatus.Allowed)
                {
                    _outputSink.Write(OutputLevel.Info, "权限", "已获得系统通知访问权限");

                    _isRunning = true;
                    _monitorThread = new Thread(monitorNotifications)
                    {
                        IsBackground = true
                    };
                    _monitorThread.Start();

                    onStatusChanged(StatusChangedEventArgs.ListenerStatus.Running, "通知监听器已启动");
                    return true;
                }
                else
                {
                    _outputSink.Write(OutputLevel.Error, "权限", "无法获得系统通知访问权限，请在系统设置中允许此应用访问通知");
                    onStatusChanged(StatusChangedEventArgs.ListenerStatus.PermissionDenied, "通知访问权限被拒绝");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _outputSink.Write(OutputLevel.Error, "启动", $"启动监听器时发生错误: {ex.Message}");
                onStatusChanged(StatusChangedEventArgs.ListenerStatus.Error, $"启动失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 停止监听器
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            onStatusChanged(StatusChangedEventArgs.ListenerStatus.Stopping, "正在停止通知监听器...");
            _isRunning = false;

            if (_monitorThread != null && _monitorThread.IsAlive)
            {
                _monitorThread.Join(3000);
            }

            onStatusChanged(StatusChangedEventArgs.ListenerStatus.Stopped, "通知监听器已停止");
        }

        #endregion

        #region private 方法 - 监测逻辑

        /// <summary>
        /// 监测通知的主循环
        /// </summary>
        private void monitorNotifications()
        {
            _outputSink.Write(OutputLevel.Info, "监测", "正在启动Windows系统通知监测...");

            if (_allowedSenderList.Count > 0)
            {
                _outputSink.Write(OutputLevel.Info, "监测", $"仅监测以下发送者的通知: {string.Join(", ", _allowedSenderList)}");
            }
            else
            {
                _outputSink.Write(OutputLevel.Info, "监测", "监测所有发送者的通知");
            }

            while (_isRunning && _listener != null)
            {
                try
                {
                    var notifications = _listener.GetNotificationsAsync(NotificationKinds.Toast)
                        .GetAwaiter().GetResult();

                    foreach (var notification in notifications)
                    {
                        processNotification(notification);
                    }
                }
                catch (Exception ex)
                {
                    _outputSink.Write(OutputLevel.Error, "监测", $"获取通知时发生错误: {ex.Message}");
                }

                cleanupCache();
                Thread.Sleep(MONITOR_INTERVAL_MS);
            }
        }

        /// <summary>
        /// 处理单个通知
        /// </summary>
        /// <param name="notification">通知对象</param>
        private void processNotification(UserNotification notification)
        {
            try
            {
                lock (_cacheLockObj)
                {
                    if (_notificationIdCache.Contains(notification.Id))
                    {
                        return;
                    }
                }

                string senderName = getSenderName(notification);
                string title = string.Empty;
                string content = getNotificationContent(notification, out title);
                DateTime notificationTime = notification.CreationTime.DateTime;

                if (_allowedSenderList.Count > 0)
                {
                    bool isAllowed = _allowedSenderList.Any(s =>
                        senderName.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                        content.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                        title.Contains(s, StringComparison.OrdinalIgnoreCase));

                    if (!isAllowed)
                    {
                        return;
                    }
                }

                DateTime filterTime = _lastReceivedNotificationTime == DateTime.MinValue
                    ? _startTime
                    : _lastReceivedNotificationTime;

                if (notificationTime <= filterTime)
                {
                    return;
                }

                lock (_cacheLockObj)
                {
                    _notificationIdCache.Add(notification.Id);
                }

                _lastReceivedNotificationTime = notificationTime;

                var notificationInfo = new NotificationInfo
                {
                    Id = notification.Id,
                    Sender = senderName,
                    Time = notificationTime,
                    Title = title,
                    Content = content
                };

                onNotificationReceived(notificationInfo);
            }
            catch (Exception ex)
            {
                _outputSink.Write(OutputLevel.Error, "处理", $"处理通知时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理通知缓存
        /// </summary>
        private void cleanupCache()
        {
            if (DateTime.Now - _lastCacheCleanupTime > TimeSpan.FromMilliseconds(CACHE_CLEANUP_INTERVAL_MS))
            {
                lock (_cacheLockObj)
                {
                    _notificationIdCache.Clear();
                }
                _lastCacheCleanupTime = DateTime.Now;
                _outputSink.Write(OutputLevel.Debug, "缓存", "通知缓存已清理");
            }
        }

        #endregion

        #region private 方法 - 辅助方法

        /// <summary>
        /// 获取发送者名称
        /// </summary>
        /// <param name="notification">通知对象</param>
        /// <returns>发送者名称</returns>
        private string getSenderName(UserNotification notification)
        {
            try
            {
                if (notification.AppInfo != null)
                {
                    return notification.AppInfo.DisplayInfo.DisplayName;
                }
                return "未知发送者";
            }
            catch
            {
                return "未知发送者";
            }
        }

        /// <summary>
        /// 获取通知内容
        /// </summary>
        /// <param name="notification">通知对象</param>
        /// <param name="title">输出标题</param>
        /// <returns>通知内容</returns>
        private string getNotificationContent(UserNotification notification, out string title)
        {
            title = string.Empty;

            try
            {
                if (notification.Notification?.Visual != null)
                {
                    var notificationBinding = notification.Notification.Visual
                        .GetBinding(KnownNotificationBindings.ToastGeneric);

                    if (notificationBinding != null)
                    {
                        var textElements = notificationBinding.GetTextElements();

                        if (textElements != null && textElements.Count > 0)
                        {
                            title = textElements.FirstOrDefault()?.Text ?? "";
                            string body = string.Join("\n", textElements.Skip(1).Select(t => t.Text));

                            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(body))
                            {
                                return body;
                            }
                            else if (!string.IsNullOrEmpty(body))
                            {
                                return body;
                            }
                            else if (!string.IsNullOrEmpty(title))
                            {
                                return title;
                            }
                        }
                    }
                }

                return "无内容";
            }
            catch
            {
                return "无法获取内容";
            }
        }

        /// <summary>
        /// 触发通知接收事件
        /// </summary>
        /// <param name="notification">通知信息</param>
        private void onNotificationReceived(NotificationInfo notification)
        {
            NotificationReceived?.Invoke(this, new NotificationEventArgs
            {
                Notification = notification
            });
        }

        /// <summary>
        /// 触发状态变更事件
        /// </summary>
        /// <param name="status">状态</param>
        /// <param name="message">消息</param>
        private void onStatusChanged(StatusChangedEventArgs.ListenerStatus status, string message)
        {
            StatusChanged?.Invoke(this, new StatusChangedEventArgs
            {
                Status = status,
                Message = message
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
            lock (_cacheLockObj)
            {
                _notificationIdCache.Clear();
            }
        }

        #endregion
    }
}
