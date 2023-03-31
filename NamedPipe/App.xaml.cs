using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO.Pipes;

namespace NamedPipeWPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                bool isNotFirstInstance = !Mutex.TryOpenExisting("NamedPipeMutex", out Mutex mutex);
                if (isNotFirstInstance)
                {
                    using (var client = new NamedPipeClientStream("NamedPipe"))
                    {
                        try
                        {
                            client.Connect();
                            using (var writer = new StreamWriter(client))
                            {
                                foreach (var arg in e.Args)
                                {
                                    writer.WriteLine(arg);
                                }
                            }
                        }
                        catch (IOException ex)
                        {
                            MessageBox.Show($"Error connecting to the named pipe client: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            Shutdown();
                        }
                    }
                    Shutdown();
                }
                else
                {
                    mutex = new Mutex(true, "NamedPipeMutex");
                    var pipeServer = new NamedPipeServerStream("NamedPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                    var pipeThread = new Thread(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                pipeServer.WaitForConnection();
                                var reader = new StreamReader(pipeServer);
                                var args = new List<string>();
                                while (!reader.EndOfStream)
                                {
                                    args.Add(reader.ReadLine());
                                }
                                pipeServer.Disconnect();
                                pipeServer.BeginWaitForConnection(null, null);
                                ShowMessageBox(args);
                            }
                            catch (IOException ex)
                            {
                                MessageBox.Show($"Error handling the named pipe connection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    });
                    pipeThread.IsBackground = true;
                    pipeThread.Start();

                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void ShowMessageBox(string[] args)
        {
            if (args.Length == 2)
            {
                MessageBox.Show($"Arg 1: {args[0]}, Arg 2: {args[1]}");
            }
            else
            {
                MessageBox.Show("Invalid number of arguments.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
