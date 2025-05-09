// StartupWindow.xaml.cs
using System.Windows;

namespace ServerDeploymentTool
{
    public partial class StartupWindow : Window
    {
        public StartupWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // Tworzenie i pokazywanie głównego okna
            var mainWindow = new MainWindow();
            mainWindow.Show();

            // Zamykanie okna startowego
            this.Close();
        }
    }
}
