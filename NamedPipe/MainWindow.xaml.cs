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
        public MainWindow()
        {
            MessageBox.Show("main window is going to load");
            InitializeComponent();
        }

    }
}
