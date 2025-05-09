// Views/ServerSettingsView.xaml.cs
using System.Windows.Controls;

namespace ServerDeploymentTool.Views
{
    public partial class ServerSettingsView : UserControl
    {
        public ServerSettingsView()
        {
            InitializeComponent();

            // Handle password box binding (since it can't be directly bound)
            Loaded += (s, e) =>
            {
                if (DataContext is ViewModels.ServerSettingsViewModel viewModel)
                {
                    PasswordBox.Password = viewModel.Settings.Password;

                    PasswordBox.PasswordChanged += (sender, args) =>
                    {
                        viewModel.Settings.Password = PasswordBox.Password;
                    };
                }
            };
        }
    }
}
