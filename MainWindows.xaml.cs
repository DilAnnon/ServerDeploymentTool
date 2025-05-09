// MainWindow.xaml.cs
using Microsoft.Extensions.DependencyInjection;
using ServerDeploymentTool.ViewModels;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace ServerDeploymentTool
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            var viewModel = App.ServiceProvider.GetRequiredService<MainViewModel>();
            DataContext = viewModel;

            // Dla debugowania
            if (DataContext == null)
            {
                System.Windows.MessageBox.Show("DataContext is null!");
            }
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            using var colorDialog = new ColorDialog();

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(_viewModel.AccentColor);
                colorDialog.Color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            }
            catch { }

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var newColor = colorDialog.Color;
                _viewModel.AccentColor = $"#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}";
            }
        }
    }
}
