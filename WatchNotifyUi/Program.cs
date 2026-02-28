namespace WatchNotifyUi
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            MainForm mainForm = new MainForm();

            if (args.Length > 0 && mainForm.HandleStartupArgument(args[0]))
            {
                return;
            }

            Application.Run(mainForm);
        }
    }
}