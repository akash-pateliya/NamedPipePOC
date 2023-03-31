using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace NamedPipe
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private const string PIPE_NAME = "NamedPipeDemo";
        private const int PIPE_TIMEOUT = 5000;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            using (var pipeClient = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.Out))
            {
                try
                {
                    // Try to connect to an existing instance of the application
                    pipeClient.Connect(PIPE_TIMEOUT);
                }
                catch (TimeoutException)
                {
                    // No existing instance found, so continue running this instance
                    StartNewInstance(args);
                    return;
                }

                // Send the command line arguments to the existing instance
                var writer = new StreamWriter(pipeClient);
                writer.AutoFlush = true;
                writer.WriteLine(args);

                // Bring the existing instance to the foreground
                var handle = Process.GetCurrentProcess().MainWindowHandle;
                SetForegroundWindow(handle);

                // Terminate this instance of the application
                Environment.Exit(0);
            }
        }

        private static void StartNewInstance(string[] args)
        {
            // Start a new instance of the application
            var processInfo = new ProcessStartInfo
            {
                FileName = AppDomain.CurrentDomain.BaseDirectory + "NamedPipe.exe",
                Arguments = string.Join(" ", args)
            };
            Process.Start(processInfo);
        }

        private void ShowMessageBox(string message)
        {
            MessageBox.Show(message, "Named Pipe Demo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
