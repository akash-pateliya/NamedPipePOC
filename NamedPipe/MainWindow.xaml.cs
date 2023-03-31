using System.Windows;

namespace NamedPipe
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            if (args.Length > 0)
            {
                try
                {
                    App.SendCommandLineArgs(args);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }
    }
}
