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

        #endregion

        #region public 构造函数

        public MainForm()
        {
            InitializeComponent();
            setupTitle();
            setupToolTip();
            loadConfig();
            bindEvents();
            updateStartupMenuState();
            updateMonitorButtonState();
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

        private void setupTitle()
        {
            Version? version = Assembly.GetExecutingAssembly().GetName().Version;
            string versionString = version?.ToString() ?? "1.0.0.0";
            string title = $"{AppTitle} v{versionString}";
            Text = title;
            ni_托盘图标.Text = title;
        }

        private void setupToolTip()
        {
            tt_提示.SetToolTip(tb_配置_消息发送地址, "格式范例：https://x.x.x.x/?text={0}\n{0} 将被替换为实际消息内容");
        }

        private void loadConfig()
        {
            _config = ConfigHelper.LoadConfig();
            applyConfigToUi();
        }

        private void applyConfigToUi()
        {
            tb_配置_消息发送地址.Text = _config.MessageUrl;
            tb_配置_监视清单.Text = string.Join(Environment.NewLine, _config.WatchList);
        }

        private AppConfig collectConfigFromUi()
        {
            AppConfig config = new()
            {
                MessageUrl = tb_配置_消息发送地址.Text.Trim()
            };

            string watchListText = tb_配置_监视清单.Text.Trim();
            if (!string.IsNullOrEmpty(watchListText))
            {
                string[] lines = watchListText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                config.WatchList = lines.Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
            }

            return config;
        }

        private void bindEvents()
        {
            bt_配置_保存.Click += bt_配置_保存_Click;
            bt_配置_还原.Click += bt_配置_还原_Click;
            ni_托盘图标.DoubleClick += ni_托盘图标_DoubleClick;
            tsmi_打开.Click += tsmi_打开_Click;
            tsmi_开机启动.Click += tsmi_开机启动_Click;
            tsmi_退出.Click += tsmi_退出_Click;
            bt_监视_启动.Click += bt_监视_启动_Click;
            bt_配置_停止.Click += bt_监视_停止_Click;
        }

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

        private void bt_配置_还原_Click(object? sender, EventArgs e)
        {
            applyConfigToUi();
            MessageBox.Show("配置已还原！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ni_托盘图标_DoubleClick(object? sender, EventArgs e)
        {
            ShowMainForm();
        }

        private void tsmi_打开_Click(object? sender, EventArgs e)
        {
            ShowMainForm();
        }

        private void tsmi_退出_Click(object? sender, EventArgs e)
        {
            RequestExit();
        }

        private void tsmi_开机启动_Click(object? sender, EventArgs e)
        {
            if (IsStartupEnabled())
            {
                if (RemoveFromStartup())
                {
                    tsmi_开机启动.Checked = false;
                }
            }
            else
            {
                if (AddToStartup())
                {
                    tsmi_开机启动.Checked = true;
                }
            }
        }

        private void updateStartupMenuState()
        {
            tsmi_开机启动.Checked = IsStartupEnabled();
        }

        private bool IsStartupEnabled()
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

        private bool AddToStartup()
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
                return TryRunWithAdminPrivileges("/registerStartup");
            }
            catch (SecurityException)
            {
                return TryRunWithAdminPrivileges("/registerStartup");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置开机启动失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool RemoveFromStartup()
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
                return TryRunWithAdminPrivileges("/unregisterStartup");
            }
            catch (SecurityException)
            {
                return TryRunWithAdminPrivileges("/unregisterStartup");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"取消开机启动失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool TryRunWithAdminPrivileges(string argument)
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

        private void ShowMainForm()
        {
            _allowShow = true;
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        #endregion

        #region private 方法 - 监视控制

        private void bt_监视_启动_Click(object? sender, EventArgs e)
        {
            startMonitoring();
        }

        private void bt_监视_停止_Click(object? sender, EventArgs e)
        {
            stopMonitoring();
        }

        private void startMonitoring()
        {
            if (_isMonitoring)
            {
                return;
            }

            if (_config.WatchList.Count == 0)
            {
                MessageBox.Show("请先配置监视清单！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            tb_监视.Clear();

            string? exePath = getMonitorExePath();
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            {
                MessageBox.Show("找不到监视程序 NotificationMonitor.exe！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
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

                appendMonitorOutput("监视程序已启动...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动监视程序失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cleanupMonitorProcess();
            }
        }

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
                appendMonitorOutput("监视程序已停止。");
            }
        }

        private void monitorProcess_Exited(object? sender, EventArgs e)
        {
            cleanupMonitorProcess();

            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    _isMonitoring = false;
                    updateMonitorButtonState();
                    appendMonitorOutput("监视程序已退出。");
                });
            }
            else
            {
                _isMonitoring = false;
                updateMonitorButtonState();
                appendMonitorOutput("监视程序已退出。");
            }
        }

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

        private void updateMonitorButtonState()
        {
            bt_监视_启动.Enabled = !_isMonitoring;
            bt_配置_停止.Enabled = _isMonitoring;
        }

        private void appendMonitorOutput(string text)
        {
            if (tb_监视.IsDisposed)
            {
                return;
            }

            tb_监视.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
            tb_监视.ScrollToCaret();
        }

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

        private ProcessStartInfo buildMonitorStartInfo(string exePath)
        {
            StringBuilder args = new();
            foreach (string sender in _config.WatchList)
            {
                args.Append($"--sender \"{sender}\" ");
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
