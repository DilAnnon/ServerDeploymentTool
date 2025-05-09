// ViewModels/RemoteTerminalViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerDeploymentTool.Services;
using System.Threading.Tasks;

namespace ServerDeploymentTool.ViewModels
{
    public class RemoteTerminalViewModel : ViewModelBase
    {
        private readonly ISshService _sshService;

        private string _command = string.Empty;
        private string _terminalOutput = string.Empty;
        private bool _isConnected;

        public string Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        public string TerminalOutput
        {
            get => _terminalOutput;
            set => SetProperty(ref _terminalOutput, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public IRelayCommand ExecuteCommandCommand { get; }
        public IRelayCommand ConnectCommand { get; }
        public IRelayCommand ClearTerminalCommand { get; }

        public RemoteTerminalViewModel(ISshService sshService)
        {
            _sshService = sshService;

            ExecuteCommandCommand = new AsyncRelayCommand(ExecuteCommandAsync, () => !string.IsNullOrWhiteSpace(Command) && IsConnected);
            ConnectCommand = new AsyncRelayCommand(ConnectAsync);
            ClearTerminalCommand = new RelayCommand(ClearTerminal);
        }

        private async Task ConnectAsync()
        {
            await ExecuteWithBusyIndicatorAsync(async () =>
            {
                var settingsService = new SettingsService();
                var settings = await settingsService.LoadSettingsAsync();

                IsConnected = await _sshService.ConnectAsync(settings.ServerSettings);

                if (IsConnected)
                {
                    AppendToTerminal("Connected to server.");
                }
                else
                {
                    AppendToTerminal("Failed to connect to server.");
                }
            }, "Connecting to server...");
        }

        private async Task ExecuteCommandAsync()
        {
            if (!IsConnected)
            {
                await ConnectAsync();
                if (!IsConnected) return;
            }

            // ViewModels/RemoteTerminalViewModel.cs (continued)
            await ExecuteWithBusyIndicatorAsync(async () =>
            {
                AppendToTerminal($"> {Command}");

                var result = await _sshService.ExecuteCommandAsync(Command);
                AppendToTerminal(result);

                Command = string.Empty;
            }, "Executing command...");
        }

        private void ClearTerminal()
        {
            TerminalOutput = string.Empty;
        }

        private void AppendToTerminal(string text)
        {
            TerminalOutput += text + "\n";
        }
    }
}

