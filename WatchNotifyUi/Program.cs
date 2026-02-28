namespace WatchNotifyUi
{
    internal static class Program
    {
        private static MainForm? _mainForm;

        [STAThread]
        static void Main(string[] args)
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ApplicationConfiguration.Initialize();
            _mainForm = new MainForm();

            if (args.Length > 0 && _mainForm.HandleStartupArgument(args[0]))
            {
                return;
            }

            Application.Run(_mainForm);
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            handleUnexpectedExit("UI线程异常", e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                handleUnexpectedExit("未处理的异常", ex);
            }
            else
            {
                handleUnexpectedExit("未处理的异常", null);
            }
        }

        private static void handleUnexpectedExit(string reason, Exception? exception)
        {
            try
            {
                _mainForm?.RequestExit();
            }
            catch
            {
            }

            if (exception != null)
            {
                MessageBox.Show(
                    $"程序因 {reason} 即将退出：\n{exception.Message}",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(
                    $"程序因 {reason} 即将退出。",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            Environment.Exit(1);
        }
    }
}