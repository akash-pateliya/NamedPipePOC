using System;
using System.Threading;
using System.Windows;

namespace NamedPipe
{
    public partial class App : Application
    {
        private Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "NamedPipeDemo";
            bool createdNew;

            mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                // An instance of the application is already running, so send the command line arguments to it
                var mainWindowHandle = NativeMethods.FindWindow(null, appName);
                NativeMethods.ShowWindow(mainWindowHandle, NativeMethods.SW_RESTORE);
                NativeMethods.SetForegroundWindow(mainWindowHandle);
                SendCommandLineArgumentsToExistingInstance(e.Args);
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        private void SendCommandLineArgumentsToExistingInstance(string[] args)
        {
            try
            {
                // Send the command line arguments to the existing instance of the application
                using (var client = new NamedPipeClientStream(".", "NamedPipeDemo"))
                {
                    client.Connect(1000);
                    var writer = new StreamWriter(client);
                    writer.AutoFlush = true;
                    writer.WriteLine(string.Join(" ", args));
                }
            }
            catch (Exception ex)
            {
                // Show a message box indicating that the named pipe is closed or disconnected
                MessageBox.Show($"Failed to send command line arguments to existing instance:\n\n{ex.Message}",
                                "Named Pipe Demo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        internal const int SW_RESTORE = 9;
    }
}
