using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NotificationMonitor.Core.Common;
using NotificationMonitor.Core.Entity;

namespace NotificationMonitor
{
    /// <summary>
    /// Windows系统通知监测工具命令行入口
    /// </summary>
    class Program
    {
        #region private 字段

        private static readonly string _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "notification_monitor.log");

        #endregion

        #region public 方法

        /// <summary>
        /// 主方法
        /// </summary>
        /// <param name="args">命令行参数</param>
        public static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            try
            {
                var options = parseCommandLineArgs(args);

                if (options.AllowedSenders.Count == 0)
                {
                    Console.WriteLine("错误: 必须指定至少一个发送者");
                    Console.WriteLine();
                    showHelp();
                    Environment.Exit(1);
                    return;
                }

                writeLog("Windows系统通知监测工具启动中...");
                Console.WriteLine("Windows系统通知监测工具启动中...");

                var outputSink = new ConsoleOutputSink();
                var listener = new NotificationListener(outputSink, options.AllowedSenders);
                var forwarder = new NtfyForwarder(options.NtfyTopic);
                var processor = new NotificationProcessor(listener, forwarder, outputSink);

                processor.NotificationProcessed += (s, e) =>
                {
                    Console.WriteLine("----------------------------------------");
                };

                bool started = await processor.StartAsync();
                if (!started)
                {
                    writeLog("通知处理器启动失败");
                    Console.WriteLine("通知处理器启动失败");
                    return;
                }

                writeLog("已开始监测系统通知，按任意键退出...");
                Console.WriteLine("已开始监测系统通知，按任意键退出...");

                Console.ReadKey();

                writeLog("正在清理资源...");
                processor.Dispose();
                forwarder.Dispose();

                writeLog("程序已退出");
                Console.WriteLine("程序已退出");
            }
            catch (Exception ex)
            {
                string errorMessage = $"程序发生未处理异常: {ex.Message}";
                writeLog(errorMessage);
                Console.WriteLine(errorMessage);
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
        }

        #endregion

        #region private 方法

        /// <summary>
        /// 记录日志到文件
        /// </summary>
        /// <param name="message">日志消息</param>
        private static void writeLog(string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now}] {message}";
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // 忽略日志写入错误
            }
        }

        /// <summary>
        /// 解析命令行参数
        /// </summary>
        /// <param name="args">命令行参数</param>
        /// <returns>解析后的选项</returns>
        private static CommandLineOptions parseCommandLineArgs(string[] args)
        {
            var options = new CommandLineOptions();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--sender" || args[i] == "-s")
                {
                    if (i + 1 < args.Length)
                    {
                        options.AllowedSenders.Add(args[i + 1]);
                        i++;
                    }
                }
                else if (args[i] == "--topic" || args[i] == "-t")
                {
                    if (i + 1 < args.Length)
                    {
                        options.NtfyTopic = args[i + 1];
                        i++;
                    }
                }
                else if (args[i] == "--help" || args[i] == "-h")
                {
                    showHelp();
                    Environment.Exit(0);
                }
            }

            return options;
        }

        /// <summary>
        /// 显示帮助信息
        /// </summary>
        private static void showHelp()
        {
            Console.WriteLine("Windows系统通知监测工具");
            Console.WriteLine("用法: NotificationMonitor.exe [选项]");
            Console.WriteLine("选项:");
            Console.WriteLine("  --sender, -s <发送者>  指定允许的发送者，可多次使用");
            Console.WriteLine("  --topic,  -t <主题>    指定ntfy.sh主题");
            Console.WriteLine("  --help,   -h           显示此帮助信息");
            Console.WriteLine("示例:");
            Console.WriteLine("  NotificationMonitor.exe --sender \"系统\"");
            Console.WriteLine("  NotificationMonitor.exe -s 系统 -s 邮件应用 -t my-topic");
        }

        #endregion

        #region 嵌套类型

        /// <summary>
        /// 命令行选项
        /// </summary>
        private class CommandLineOptions
        {
            #region public 属性

            /// <summary>
            /// 获取允许的发送者列表
            /// </summary>
            public List<string> AllowedSenders { get; } = new List<string>();

            /// <summary>
            /// 获取或设置ntfy主题
            /// </summary>
            public string NtfyTopic { get; set; } = string.Empty;

            #endregion
        }

        #endregion
    }
}
