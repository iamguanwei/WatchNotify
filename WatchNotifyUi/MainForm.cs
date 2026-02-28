using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Text;
using WatchNotifyUi.Entity;
using WatchNotifyUi.Helper;

namespace WatchNotifyUi
{
    public partial class MainForm : Form
    {
        #region private 字段

        private AppConfig _config = new();

        private bool _allowClose = false;

        private bool _allowShow = false;

        private const string StartupRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "NotificationMonitor";
        private const string AppTitle = "通知监视器";

        private Process? _monitorProcess;

        private bool _isMonitoring = false;

        private readonly object _processLockObj = new();

        private readonly StringBuilder _pendingOutput = new();

        #endregion

        #region public 构造函数

        public MainForm()
        {
            InitializeComponent();
            setTitle();
            setToolTip();
            loadConfiguration();
            bindEvents();
            updateStartupMenuState();
            updateMonitorMenuState();
            updateMonitorButtonState();
            autoStartMonitorIfNeeded();
        }

        #endregion

        #region internal 方法

        internal void RequestExit()
        {
            _allowClose = true;
            stopMonitoring();
            ni_托盘图标.Visible = false;
            Application.Exit();
        }

        #endregion

        #region protected 方法

        protected override void SetVisibleCore(bool value)
        {
            if (!_allowShow)
            {
                value = false;
            }
            base.SetVisibleCore(value);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_allowClose)
            {
                e.Cancel = true;
                Hide();
            }
            base.OnFormClosing(e);
        }

        #endregion

        #region private 方法

        /// <summary>
        /// 设置窗口标题，包括版本号信息。
        /// </summary>
        private void setTitle()
        {
            Version? version = Assembly.GetExecutingAssembly().GetName().Version;
            string versionString = version?.ToString() ?? "1.0.0.0";
            string title = $"{AppTitle} v{versionString}";
            Text = title;
            ni_托盘图标.Text = title;
        }

        /// <summary>
        /// 设置控件的工具提示信息。
        /// </summary>
        private void setToolTip()
        {
            tt_提示.SetToolTip(tb_配置_消息发送地址, "格式范例：https://x.x.x.x/?text={0}\n{0} 将被替换为实际消息内容");
        }

        /// <summary>
        /// 从文件加载配置到内存。
        /// </summary>
        private void loadConfiguration()
        {
            _config = ConfigHelper.LoadConfig();
            applyConfigToUi();
        }

        /// <summary>
        /// 将配置应用到UI控件。
        /// </summary>
        private void applyConfigToUi()
        {
            tb_配置_消息发送地址.Text = _config.MessageUrl;
            tb_配置_监视清单.Text = string.Join(Environment.NewLine, _config.WatchList);
        }

        /// <summary>
        /// 从UI收集配置信息。
        /// </summary>
        /// <returns>从UI收集的配置对象。</returns>
        private AppConfig collectConfigFromUi()
        {
            AppConfig config = new()
            {
                MessageUrl = tb_配置_消息发送地址.Text.Trim(),
                AutoStartMonitor = _config.AutoStartMonitor
            };

            string watchListText = tb_配置_监视清单.Text.Trim();
            if (!string.IsNullOrEmpty(watchListText))
            {
                string[] lines = watchListText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                config.WatchList = lines.Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
            }

            return config;
        }

        /// <summary>
        /// 绑定UI控件的事件处理程序。
        /// </summary>
        private void bindEvents()
        {
            bt_配置_保存.Click += bt_配置_保存_Click;
            bt_配置_还原.Click += bt_配置_还原_Click;
            ni_托盘图标.DoubleClick += ni_托盘图标_DoubleClick;
            tsmi_打开.Click += tsmi_打开_Click;
            tsmi_开启监控.Click += tsmi_开启监控_Click;
            tsmi_开机启动.Click += tsmi_开机启动_Click;
            tsmi_退出.Click += tsmi_退出_Click;
            bt_监视_启动.Click += bt_监视_启动_Click;
            bt_配置_停止.Click += bt_监视_停止_Click;
            bt_配置_测试发送地址.Click += bt_配置_测试发送地址_Click;
        }

        /// <summary>
        /// 保存配置按钮的点击事件处理程序。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void bt_配置_保存_Click(object? sender, EventArgs e)
        {
            AppConfig newConfig = collectConfigFromUi();

            if (ConfigHelper.SaveConfig(newConfig))
            {
                _config = newConfig;
                MessageBox.Show("配置保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("配置保存失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 还原配置按钮的点击事件处理程序。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void bt_配置_还原_Click(object? sender, EventArgs e)
        {
            applyConfigToUi();
            MessageBox.Show("配置已还原！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 测试发送地址按钮的点击事件处理程序。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private async void bt_配置_测试发送地址_Click(object? sender, EventArgs e)
        {
            string testUrl = tb_配置_消息发送地址.Text.Trim();

            if (string.IsNullOrEmpty(testUrl))
            {
                MessageBox.Show("请先输入消息发送地址！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bt_配置_测试发送地址.Enabled = false;
            bt_配置_测试发送地址.Text = "测试中...";

            try
            {
                string? exePath = getMonitorExePath();
                if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                {
                    MessageBox.Show("找不到监视程序 NotificationMonitor.exe！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string result = await runTestAsync(exePath, testUrl);
                bool isSuccess = result.Contains("测试成功");

                MessageBox.Show(result, "测试结果", MessageBoxButtons.OK, isSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"测试过程中发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                bt_配置_测试发送地址.Enabled = true;
                bt_配置_测试发送地址.Text = "测试";
            }
        }

        /// <summary>
        /// 运行测试发送地址的异步方法。
        /// </summary>
        /// <param name="exePath">监视程序路径。</param>
        /// <param name="testUrl">测试URL地址。</param>
        /// <returns>测试结果字符串。</returns>
        private async Task<string> runTestAsync(string exePath, string testUrl)
        {
            StringBuilder output = new();
            StringBuilder error = new();

            ProcessStartInfo startInfo = new()
            {
                FileName = exePath,
                Arguments = $"--test \"{testUrl}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using Process process = new() { StartInfo = startInfo };
            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.AppendLine(e.Data);
                }
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    error.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            string result = output.ToString();
            if (error.Length > 0)
            {
                result += error.ToString();
            }

            return result.Trim();
        }

        /// <summary>
        /// 托盘图标双击事件处理程序，显示主窗口。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void ni_托盘图标_DoubleClick(object? sender, EventArgs e)
        {
            showMainForm();
        }

        /// <summary>
        /// 打开菜单项点击事件处理程序，显示主窗口。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void tsmi_打开_Click(object? sender, EventArgs e)
        {
            showMainForm();
        }

        /// <summary>
        /// 开启监控菜单项点击事件处理程序。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void tsmi_开启监控_Click(object? sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => tsmi_开启监控_Click(sender, e)));
                return;
            }

            if (_isMonitoring)
            {
                stopMonitoring();
                tsmi_开启监控.Checked = false;
                saveAutoStartMonitorConfig(false);
            }
            else
            {
                if (startMonitoring())
                {
                    tsmi_开启监控.Checked = true;
                    saveAutoStartMonitorConfig(true);
                }
            }
        }

        /// <summary>
        /// 退出菜单项点击事件处理程序。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void tsmi_退出_Click(object? sender, EventArgs e)
        {
            RequestExit();
        }

        /// <summary>
        /// 开机启动菜单项点击事件处理程序。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void tsmi_开机启动_Click(object? sender, EventArgs e)
        {
            if (isStartupEnabled())
            {
                if (removeFromStartup())
                {
                    tsmi_开机启动.Checked = false;
                }
            }
            else
            {
                if (addToStartup())
                {
                    tsmi_开机启动.Checked = true;
                }
            }
        }

        /// <summary>
        /// 更新开机启动菜单的勾选状态。
        /// </summary>
        private void updateStartupMenuState()
        {
            tsmi_开机启动.Checked = isStartupEnabled();
        }

        /// <summary>
        /// 更新监控菜单的勾选状态。
        /// </summary>
        private void updateMonitorMenuState()
        {
            if (InvokeRequired)
            {
                Invoke(updateMonitorMenuState);
                return;
            }

            tsmi_开启监控.Checked = _isMonitoring;
        }

        /// <summary>
        /// 保存自动启动监控配置。
        /// </summary>
        /// <param name="autoStart">是否自动启动。</param>
        private void saveAutoStartMonitorConfig(bool autoStart)
        {
            _config.AutoStartMonitor = autoStart;
            ConfigHelper.SaveConfig(_config);
        }

        /// <summary>
        /// 如果配置了自动启动监控，则启动监控。
        /// </summary>
        private void autoStartMonitorIfNeeded()
        {
            if (_config.AutoStartMonitor)
            {
                tsmi_开启监控.Checked = true;
                startMonitoring();
            }
        }

        /// <summary>
        /// 检查是否已启用开机启动。
        /// </summary>
        /// <returns>已启用返回true，否则返回false。</returns>
        private bool isStartupEnabled()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, false);
                string? value = key?.GetValue(AppName) as string;
                return !string.IsNullOrEmpty(value);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 添加应用到开机启动项。
        /// </summary>
        /// <returns>成功返回true，否则返回false。</returns>
        private bool addToStartup()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, true);
                if (key == null)
                {
                    MessageBox.Show("无法打开注册表项！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                string exePath = Environment.ProcessPath ?? Application.ExecutablePath;
                key.SetValue(AppName, $"\"{exePath}\"");
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return tryRunWithAdminPrivileges("/registerStartup");
            }
            catch (SecurityException)
            {
                return tryRunWithAdminPrivileges("/registerStartup");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置开机启动失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 从开机启动项移除应用。
        /// </summary>
        /// <returns>成功返回true，否则返回false。</returns>
        private bool removeFromStartup()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, true);
                if (key == null)
                {
                    return true;
                }

                key.DeleteValue(AppName, false);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return tryRunWithAdminPrivileges("/unregisterStartup");
            }
            catch (SecurityException)
            {
                return tryRunWithAdminPrivileges("/unregisterStartup");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"取消开机启动失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 尝试以管理员权限运行应用。
        /// </summary>
        /// <param name="argument">启动参数。</param>
        /// <returns>成功返回true，否则返回false。</returns>
        private bool tryRunWithAdminPrivileges(string argument)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "设置开机启动需要管理员权限，是否以管理员身份重新启动？",
                    "需要管理员权限",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return false;
                }

                string? exePath = Environment.ProcessPath ?? Application.ExecutablePath;
                ProcessStartInfo startInfo = new()
                {
                    FileName = exePath,
                    Arguments = argument,
                    Verb = "runas",
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                RequestExit();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动管理员进程失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        internal bool HandleStartupArgument(string argument)
        {
            if (argument.Equals("/registerStartup", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, true);
                    if (key != null)
                    {
                        string exePath = Environment.ProcessPath ?? Application.ExecutablePath;
                        key.SetValue(AppName, $"\"{exePath}\"");
                    }
                }
                catch { }
                return true;
            }

            if (argument.Equals("/unregisterStartup", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, true);
                    key?.DeleteValue(AppName, false);
                }
                catch { }
                return true;
            }

            return false;
        }

        /// <summary>
        /// 显示主窗口。
        /// </summary>
        private void showMainForm()
        {
            _allowShow = true;
            Show();
            WindowState = FormWindowState.Normal;
            Activate();

            if (_pendingOutput.Length > 0 && !tb_监视.IsDisposed)
            {
                tb_监视.AppendText(_pendingOutput.ToString());
                tb_监视.ScrollToCaret();
                _pendingOutput.Clear();
            }
        }

        #endregion

        #region private 方法 - 监视控制

        /// <summary>
        /// 启动监视按钮的点击事件处理程序。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void bt_监视_启动_Click(object? sender, EventArgs e)
        {
            startMonitoring();
        }

        /// <summary>
        /// 停止监视按钮的点击事件处理程序。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void bt_监视_停止_Click(object? sender, EventArgs e)
        {
            stopMonitoring();
        }

        /// <summary>
        /// 启动监控服务。
        /// </summary>
        /// <returns>启动成功返回true，否则返回false。</returns>
        private bool startMonitoring()
        {
            if (_isMonitoring)
            {
                return true;
            }

            if (_config.WatchList.Count == 0)
            {
                MessageBox.Show("请先配置监视清单！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(_config.MessageUrl))
            {
                MessageBox.Show("请先配置消息发送地址！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            clearMonitorOutput();

            string? exePath = getMonitorExePath();
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            {
                MessageBox.Show("找不到监视程序 NotificationMonitor.exe！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                ProcessStartInfo startInfo = buildMonitorStartInfo(exePath);
                lock (_processLockObj)
                {
                    _monitorProcess = new Process
                    {
                        StartInfo = startInfo,
                        EnableRaisingEvents = true
                    };
                    _monitorProcess.Exited += monitorProcess_Exited;
                    _monitorProcess.OutputDataReceived += monitorProcess_OutputDataReceived;
                    _monitorProcess.ErrorDataReceived += monitorProcess_ErrorDataReceived;
                }

                _monitorProcess.Start();
                _monitorProcess.BeginOutputReadLine();
                _monitorProcess.BeginErrorReadLine();

                _isMonitoring = true;
                updateMonitorButtonState();
                updateMonitorMenuState();

                appendMonitorOutput("监视程序已启动...");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动监视程序失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cleanupMonitorProcess();
                return false;
            }
        }

        /// <summary>
        /// 停止监控服务。
        /// </summary>
        private void stopMonitoring()
        {
            if (!_isMonitoring)
            {
                return;
            }

            try
            {
                lock (_processLockObj)
                {
                    if (_monitorProcess != null && !_monitorProcess.HasExited)
                    {
                        _monitorProcess.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                appendMonitorOutput($"停止监视程序时发生错误：{ex.Message}");
            }
            finally
            {
                cleanupMonitorProcess();
                _isMonitoring = false;
                updateMonitorButtonState();
                updateMonitorMenuState();
                appendMonitorOutput("监视程序已停止。");
            }
        }

        /// <summary>
        /// 监视进程退出事件处理程序。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void monitorProcess_Exited(object? sender, EventArgs e)
        {
            cleanupMonitorProcess();

            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    _isMonitoring = false;
                    updateMonitorButtonState();
                    updateMonitorMenuState();
                    appendMonitorOutput("监视程序已退出。");
                });
            }
            else
            {
                _isMonitoring = false;
                updateMonitorButtonState();
                updateMonitorMenuState();
                appendMonitorOutput("监视程序已退出。");
            }
        }

        /// <summary>
        /// 监视进程输出数据接收事件处理程序。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">数据接收事件参数。</param>
        private void monitorProcess_OutputDataReceived(object? sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (InvokeRequired)
                {
                    Invoke(() => appendMonitorOutput(e.Data));
                }
                else
                {
                    appendMonitorOutput(e.Data);
                }
            }
        }

        /// <summary>
        /// 监视进程错误数据接收事件处理程序。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">数据接收事件参数。</param>
        private void monitorProcess_ErrorDataReceived(object? sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (InvokeRequired)
                {
                    Invoke(() => appendMonitorOutput($"[错误] {e.Data}"));
                }
                else
                {
                    appendMonitorOutput($"[错误] {e.Data}");
                }
            }
        }

        /// <summary>
        /// 清理监视进程资源。
        /// </summary>
        private void cleanupMonitorProcess()
        {
            lock (_processLockObj)
            {
                if (_monitorProcess != null)
                {
                    try
                    {
                        _monitorProcess.OutputDataReceived -= monitorProcess_OutputDataReceived;
                        _monitorProcess.ErrorDataReceived -= monitorProcess_ErrorDataReceived;
                        _monitorProcess.Exited -= monitorProcess_Exited;
                        _monitorProcess.CancelOutputRead();
                        _monitorProcess.CancelErrorRead();
                        _monitorProcess.Dispose();
                    }
                    catch { }
                    _monitorProcess = null;
                }
            }
        }

        /// <summary>
        /// 更新监视按钮的启用/禁用状态。
        /// </summary>
        private void updateMonitorButtonState()
        {
            if (InvokeRequired)
            {
                Invoke(updateMonitorButtonState);
                return;
            }

            bt_监视_启动.Enabled = !_isMonitoring;
            bt_配置_停止.Enabled = _isMonitoring;
        }

        /// <summary>
        /// 向监视输出文本框追加文本。
        /// </summary>
        /// <param name="text">要追加的文本。</param>
        private void appendMonitorOutput(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => appendMonitorOutput(text)));
                return;
            }

            string formattedText = $"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}";

            if (tb_监视.IsDisposed)
            {
                return;
            }

            if (!IsHandleCreated || !Visible)
            {
                _pendingOutput.Append(formattedText);
                return;
            }

            tb_监视.AppendText(formattedText);
            tb_监视.ScrollToCaret();
        }

        /// <summary>
        /// 清空监视输出文本框。
        /// </summary>
        private void clearMonitorOutput()
        {
            if (InvokeRequired)
            {
                Invoke(clearMonitorOutput);
                return;
            }

            if (tb_监视.IsDisposed)
            {
                return;
            }

            tb_监视.Clear();
        }

        /// <summary>
        /// 获取监视程序的完整路径。
        /// </summary>
        /// <returns>监视程序路径，如果未找到则返回null。</returns>
        private string? getMonitorExePath()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string exePath = Path.Combine(currentDir, "NotificationMonitor.exe");
            if (File.Exists(exePath))
            {
                return exePath;
            }

            string solutionDir = Path.GetFullPath(Path.Combine(currentDir, @"..\..\..\..\"));
            string projectPath = Path.Combine(solutionDir, "NotificationMonitor");
            string buildPath = Path.Combine(projectPath, "bin", "Debug", "net9.0-windows10.0.22000.0", "NotificationMonitor.exe");
            if (File.Exists(buildPath))
            {
                return buildPath;
            }

            return null;
        }

        /// <summary>
        /// 构建监视程序的启动信息。
        /// </summary>
        /// <param name="exePath">监视程序路径。</param>
        /// <returns>进程启动信息对象。</returns>
        private ProcessStartInfo buildMonitorStartInfo(string exePath)
        {
            StringBuilder args = new();
            foreach (string sender in _config.WatchList)
            {
                args.Append($"--sender \"{sender}\" ");
            }

            if (!string.IsNullOrEmpty(_config.MessageUrl))
            {
                args.Append($"--url \"{_config.MessageUrl}\" ");
            }

            return new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args.ToString().Trim(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
        }

        #endregion
    }
}
