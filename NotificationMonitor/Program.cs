using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
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
        #region 常量

        private const string TEST_MESSAGE = "这是一条测试消息，用于验证转发地址是否正常工作。";
        private const string MUTEX_NAME = "Global\\NotificationMonitor_SingleInstance_Mutex";

        #endregion

        #region private 字段

        private static readonly string _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "notification_monitor.log");
        private static bool _enableLog = false;

        #endregion

        #region public 方法

        /// <summary>
        /// 主方法
        /// </summary>
        /// <param name="args">命令行参数</param>
        public static async Task Main(string[] args)
        {
            using var mutex = new Mutex(true, MUTEX_NAME, out bool createdNew);
            if (!createdNew)
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.WriteLine("另一个实例正在运行，程序即将退出...");
                return;
            }

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            try
            {
                var options = parseCommandLineArgs(args);
                _enableLog = options.EnableLog;

                showVersion();

                if (options.IsTestMode)
                {
                    await runTestModeAsync(options);
                    return;
                }

                if (options.AllowedSenders.Count == 0)
                {
                    Console.WriteLine("错误: 必须指定至少一个发送者");
                    Console.WriteLine();
                    showHelp();
                    Environment.Exit(1);
                    return;
                }

                if (string.IsNullOrEmpty(options.ForwardUrl))
                {
                    Console.WriteLine("错误: 必须指定转发URL");
                    Console.WriteLine();
                    showHelp();
                    Environment.Exit(1);
                    return;
                }

                writeLog("Windows系统通知监测工具启动中...");
                Console.WriteLine("正在启动...");

                var outputSink = new ConsoleOutputSink();
                var listener = new NotificationListener(outputSink, options.AllowedSenders);
                var forwarder = new HttpForwarder(options.ForwardUrl);
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
        /// 显示版本信息
        /// </summary>
        private static void showVersion()
        {
            Version? version = Assembly.GetExecutingAssembly().GetName().Version;
            string versionString = version?.ToString() ?? "1.0.0.0";
            Console.WriteLine($"Windows系统通知监测工具 v{versionString}");
            Console.WriteLine();
        }

        /// <summary>
        /// 记录日志到文件
        /// </summary>
        /// <param name="message">日志消息</param>
        private static void writeLog(string message)
        {
            if (!_enableLog)
            {
                return;
            }

            try
            {
                string logEntry = $"[{DateTime.Now}] {message}";
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
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
                else if (args[i] == "--url" || args[i] == "-u")
                {
                    if (i + 1 < args.Length)
                    {
                        options.ForwardUrl = args[i + 1];
                        i++;
                    }
                }
                else if (args[i] == "--log" || args[i] == "-l")
                {
                    options.EnableLog = true;
                }
                else if (args[i] == "--test")
                {
                    options.IsTestMode = true;
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        options.TestUrl = args[i + 1];
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
        /// 运行测试模式
        /// </summary>
        /// <param name="options">命令行选项</param>
        private static async Task runTestModeAsync(CommandLineOptions options)
        {
            Console.WriteLine("正在测试转发地址...");

            string testUrl = options.TestUrl;
            if (string.IsNullOrEmpty(testUrl))
            {
                testUrl = options.ForwardUrl;
            }

            if (string.IsNullOrEmpty(testUrl))
            {
                Console.WriteLine("测试失败！未指定转发地址");
                return;
            }

            await testForwardUrlAsync(testUrl);
        }

        /// <summary>
        /// 测试转发URL
        /// </summary>
        /// <param name="url">转发URL</param>
        private static async Task testForwardUrlAsync(string url)
        {
            Console.WriteLine($"测试地址: {url}");

            try
            {
                string testUrl = url.Contains("{0}")
                    ? string.Format(url, Uri.EscapeDataString(TEST_MESSAGE))
                    : url;

                using var httpClient = new HttpClient();
                var content = new StringContent(TEST_MESSAGE, Encoding.UTF8, "text/plain");
                var response = await httpClient.PostAsync(testUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"测试成功！HTTP状态码: {(int)response.StatusCode} {response.StatusCode}");
                }
                else
                {
                    Console.WriteLine($"测试失败！HTTP状态码: {(int)response.StatusCode} {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"测试失败！网络错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败！发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示帮助信息
        /// </summary>
        private static void showHelp()
        {
            showVersion();
            Console.WriteLine("用法: NotificationMonitor.exe [选项]");
            Console.WriteLine("选项:");
            Console.WriteLine("  --sender, -s <发送者>  指定允许的发送者，可多次使用");
            Console.WriteLine("  --url,    -u <地址>    指定转发URL，支持{0}占位符替换消息内容");
            Console.WriteLine("  --log,    -l           启用日志记录到文件");
            Console.WriteLine("  --test [地址]          测试转发地址，不指定则使用--url参数的地址");
            Console.WriteLine("  --help,   -h           显示此帮助信息");
            Console.WriteLine("示例:");
            Console.WriteLine("  NotificationMonitor.exe --sender \"系统\" --url https://example.com/api");
            Console.WriteLine("  NotificationMonitor.exe -s 系统 -s 邮件应用 -u https://example.com/msg?text={0} -l");
            Console.WriteLine("  NotificationMonitor.exe --test https://example.com/api");
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
            /// 获取或设置转发URL
            /// </summary>
            public string ForwardUrl { get; set; } = string.Empty;

            /// <summary>
            /// 获取或设置是否为测试模式
            /// </summary>
            public bool IsTestMode { get; set; }

            /// <summary>
            /// 获取或设置测试URL
            /// </summary>
            public string TestUrl { get; set; } = string.Empty;

            /// <summary>
            /// 获取或设置是否启用日志
            /// </summary>
            public bool EnableLog { get; set; }

            #endregion
        }

        #endregion
    }
}
