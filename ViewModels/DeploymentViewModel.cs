// ViewModels/DeploymentViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerDeploymentTool.Models;
using ServerDeploymentTool.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerDeploymentTool.ViewModels
{
    public class DeploymentViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ISshService _sshService;
        private readonly IFileComparisonService _fileComparisonService;

        private string _localFolderPath = string.Empty;
        private int _currentProgress;
        private int _totalProgress;
        private string _currentFile = string.Empty;
        private bool _isConnected;
        private bool _hasChanges;
        private string _logOutput = string.Empty;

        public string LocalFolderPath
        {
            get => _localFolderPath;
            set => SetProperty(ref _localFolderPath, value);
        }

        public int CurrentProgress
        {
            get => _currentProgress;
            set => SetProperty(ref _currentProgress, value);
        }

        public int TotalProgress
        {
            get => _totalProgress;
            set => SetProperty(ref _totalProgress, value);
        }

        public string CurrentFile
        {
            get => _currentFile;
            set => SetProperty(ref _currentFile, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public bool HasChanges
        {
            get => _hasChanges;
            set => SetProperty(ref _hasChanges, value);
        }

        public string LogOutput
        {
            get => _logOutput;
            set => SetProperty(ref _logOutput, value);
        }

        public ObservableCollection<FileComparisonResult> ChangedFiles { get; } = new();

        public IRelayCommand SelectFolderCommand { get; }
        public IRelayCommand ConnectCommand { get; }
        public IRelayCommand CompareFilesCommand { get; }
        public IRelayCommand DeployCommand { get; }

        public DeploymentViewModel(
            ISettingsService settingsService,
            ISshService sshService,
            IFileComparisonService fileComparisonService)
        {
            _settingsService = settingsService;
            _sshService = sshService;
            _fileComparisonService = fileComparisonService;

            SelectFolderCommand = new RelayCommand(SelectFolder);
            ConnectCommand = new AsyncRelayCommand(ConnectAsync);
            CompareFilesCommand = new AsyncRelayCommand(CompareFilesAsync);
            DeployCommand = new AsyncRelayCommand(DeployAsync);

            LoadLastFolderAsync().ConfigureAwait(false);
        }

        private async Task LoadLastFolderAsync()
        {
            var settings = await _settingsService.LoadSettingsAsync();
            LocalFolderPath = settings.LastLocalFolderPath;
        }

        private void SelectFolder()
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Select local folder to deploy",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                LocalFolderPath = folderDialog.SelectedPath;
                SaveLastFolderAsync().ConfigureAwait(false);
            }
        }

        private async Task SaveLastFolderAsync()
        {
            var settings = await _settingsService.LoadSettingsAsync();
            settings.LastLocalFolderPath = LocalFolderPath;
            await _settingsService.SaveSettingsAsync(settings);
        }

        private async Task ConnectAsync()
        {
            await ExecuteWithBusyIndicatorAsync(async () =>
            {
                var settings = await _settingsService.LoadSettingsAsync();
                IsConnected = await _sshService.ConnectAsync(settings.ServerSettings);

                if (IsConnected)
                {
                    AddToLog("Connected to server successfully.");
                }
                else
                {
                    AddToLog("Failed to connect to server.");
                }
            }, "Connecting to server...");
        }

        private async Task CompareFilesAsync()
        {
            if (!IsConnected)
            {
                await ConnectAsync();
                if (!IsConnected) return;
            }

            await ExecuteWithBusyIndicatorAsync(async () =>
            {
                ChangedFiles.Clear();

                var settings = await _settingsService.LoadSettingsAsync();
                var results = await _fileComparisonService.CompareFilesAsync(
                    LocalFolderPath,
                    settings.ServerSettings.ServerPath,
                    _sshService);

                var changedFiles = results.Where(r => r.IsModified).ToList();

                foreach (var file in changedFiles)
                {
                    ChangedFiles.Add(file);
                }

                HasChanges = ChangedFiles.Count > 0;

                AddToLog($"Found {ChangedFiles.Count} files that need to be updated.");
            }, "Comparing files...");
        }

        private async Task DeployAsync()
        {
            if (!IsConnected)
            {
                await ConnectAsync();
                if (!IsConnected) return;
            }

            await ExecuteWithBusyIndicatorAsync(async () =>
            {
                var settings = await _settingsService.LoadSettingsAsync();

                // Stop the application
                AddToLog($"Stopping application: {settings.ServerSettings.AppName}");
                await _sshService.StopApplicationAsync(settings.ServerSettings.AppName);

                // Delete remote directory
                AddToLog($"Deleting remote directory: {settings.ServerSettings.ServerPath}");
                await _sshService.DeleteRemoteDirectoryAsync(settings.ServerSettings.ServerPath);

                // Upload files
                AddToLog("Uploading files...");
                var progress = new Progress<(int current, int total, string fileName)>(report =>
                {
                    CurrentProgress = report.current;
                    TotalProgress = report.total;
                    CurrentFile = report.fileName;

                    AddToLog($"Uploading ({report.current}/{report.total}): {report.fileName}");
                });

                await _sshService.UploadDirectoryAsync(
                    LocalFolderPath,
                    settings.ServerSettings.ServerPath,
                    progress);

                // Run npm install
                AddToLog("Running npm install...");
                await _sshService.RunNpmInstallAsync(settings.ServerSettings.ServerPath);

                // Start the application
                AddToLog($"Starting application: {settings.ServerSettings.AppName}");
                await _sshService.StartApplicationAsync(settings.ServerSettings.AppName);

                AddToLog("Deployment completed successfully!");

                // Refresh file comparison
                await CompareFilesAsync();
            }, "Deploying application...");
        }

        private void AddToLog(string message)
        {
            LogOutput += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
        }
    }
}
