using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NamedPipe
{
    public partial class App : Application
    {
        private const string PipeName = "MyAppPipe";
        private NamedPipeServerStream pipeServer;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1,
                PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            var pipeConnected = false;
            try
            {
                pipeServer.BeginWaitForConnection(ar =>
                {
                    pipeServer.EndWaitForConnection(ar);
                    pipeConnected = true;
                }, null);
            }
            catch (Exception)
            {
                pipeServer.Dispose();
                pipeServer = null;
            }

            if (!pipeConnected)
            {
                // New instance, create pipe and start server
                base.OnStartup(e);
                return;
            }

            // Existing instance, read command line arguments from pipe
            using (var reader = new StreamReader(pipeServer))
            {
                var args = reader.ReadToEnd();
                var argArray = args.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (argArray.Length > 0)
                {
                    string arg1 = argArray[0];
                    string arg2 = argArray.Length > 1 ? argArray[1] : null;
                    try
                    {
                        var window = Current.Windows.OfType<MainWindow>().FirstOrDefault();
                        window?.Dispatcher.Invoke(() =>
                        {
                            window.ShowMessage($"arg1: {arg1}, arg2: {arg2}");
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
            }

            // Exit new instance
            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Clean up named pipe server
            pipeServer?.Dispose();
            base.OnExit(e);
        }

        internal static void SendCommandLineArgs(string[] args)
        {
            using (var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
            {
                try
                {
                    pipeClient.Connect(5000);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                    return;
                }

                using (var writer = new StreamWriter(pipeClient))
                {
                    writer.Write(string.Join("|", args));
                }
            }
        }
    }
}
