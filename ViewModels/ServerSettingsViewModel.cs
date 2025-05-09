// ViewModels/ServerSettingsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ServerDeploymentTool.Models;
using ServerDeploymentTool.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerDeploymentTool.ViewModels
{
    public class ServerSettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ISshService _sshService;
        private ServerSettings _settings = new();

        public ServerSettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        public IRelayCommand TestConnectionCommand { get; }
        public IRelayCommand SaveSettingsCommand { get; }
        public IRelayCommand BrowsePrivateKeyCommand { get; }

        public ServerSettingsViewModel(ISettingsService settingsService, ISshService sshService)
        {
            _settingsService = settingsService;
            _sshService = sshService;

            TestConnectionCommand = new AsyncRelayCommand(TestConnectionAsync);
            SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);
            BrowsePrivateKeyCommand = new RelayCommand(BrowsePrivateKey);

            LoadSettingsAsync().ConfigureAwait(false);
        }

        private async Task LoadSettingsAsync()
        {
            var appSettings = await _settingsService.LoadSettingsAsync();
            Settings = appSettings.ServerSettings;
        }

        private async Task TestConnectionAsync()
        {
            await ExecuteWithBusyIndicatorAsync(async () =>
            {
                StatusMessage = "Testing connection...";
                var result = await _sshService.TestConnectionAsync(Settings);
                StatusMessage = result ? "Connection successful!" : "Connection failed!";
            });
        }

        private async Task SaveSettingsAsync()
        {
            await ExecuteWithBusyIndicatorAsync(async () =>
            {
                var appSettings = await _settingsService.LoadSettingsAsync();
                appSettings.ServerSettings = Settings;
                await _settingsService.SaveSettingsAsync(appSettings);
                StatusMessage = "Settings saved successfully!";
            });
        }

        private void BrowsePrivateKey()
        {
            using var openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "Select SSH Private Key File",
                Filter = "All Files (*.*)|*.*",
                CheckFileExists = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Settings.PrivateKeyPath = openFileDialog.FileName;
                OnPropertyChanged(nameof(Settings));
            }
        }
    }
}
