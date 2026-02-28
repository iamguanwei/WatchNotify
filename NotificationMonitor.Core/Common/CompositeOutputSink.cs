using System;
using System.Collections.Generic;
using NotificationMonitor.Core.Entity;
using NotificationMonitor.Core.Interface;

namespace NotificationMonitor.Core.Common
{
    /// <summary>
    /// 复合输出接收器
    /// 支持同时向多个输出目标发送信息
    /// </summary>
    public class CompositeOutputSink : IOutputSink
    {
        #region private 字段

        private readonly List<IOutputSink> _sinkList;
        private readonly object _lockObj;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化复合输出接收器实例
        /// </summary>
        public CompositeOutputSink()
        {
            _sinkList = new List<IOutputSink>();
            _lockObj = new object();
        }

        /// <summary>
        /// 初始化复合输出接收器实例并添加指定的输出接收器
        /// </summary>
        /// <param name="sinks">初始输出接收器集合</param>
        public CompositeOutputSink(IEnumerable<IOutputSink> sinks)
        {
            _sinkList = new List<IOutputSink>(sinks);
            _lockObj = new object();
        }

        #endregion

        #region public 方法

        /// <summary>
        /// 添加输出接收器
        /// </summary>
        /// <param name="sink">输出接收器实例</param>
        public void AddSink(IOutputSink sink)
        {
            if (sink == null)
            {
                return;
            }

            lock (_lockObj)
            {
                _sinkList.Add(sink);
            }
        }

        /// <summary>
        /// 移除输出接收器
        /// </summary>
        /// <param name="sink">输出接收器实例</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveSink(IOutputSink sink)
        {
            if (sink == null)
            {
                return false;
            }

            lock (_lockObj)
            {
                return _sinkList.Remove(sink);
            }
        }

        /// <summary>
        /// 清空所有输出接收器
        /// </summary>
        public void ClearSinks()
        {
            lock (_lockObj)
            {
                _sinkList.Clear();
            }
        }

        #endregion

        #region IOutputSink 实现

        /// <summary>
        /// 向所有注册的输出接收器发送信息
        /// </summary>
        /// <param name="level">输出级别</param>
        /// <param name="category">输出分类</param>
        /// <param name="message">消息内容</param>
        public void Write(OutputLevel level, string category, string message)
        {
            List<IOutputSink> sinksCopy;
            lock (_lockObj)
            {
                sinksCopy = new List<IOutputSink>(_sinkList);
            }

            foreach (var sink in sinksCopy)
            {
                try
                {
                    sink.Write(level, category, message);
                }
                catch
                {
                    // 忽略单个sink的错误，继续向其他sink发送
                }
            }
        }

        #endregion
    }
}
