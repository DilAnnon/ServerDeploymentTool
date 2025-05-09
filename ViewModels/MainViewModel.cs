using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerDeploymentTool.Models;
using ServerDeploymentTool.Services;
using ServerDeploymentTool.Themes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ServerDeploymentTool.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ILocalizationService _localizationService;
        private readonly ServerSettingsViewModel _serverSettingsViewModel;
        private readonly DeploymentViewModel _deploymentViewModel;
        private readonly RemoteTerminalViewModel _remoteTerminalViewModel;

        private ViewModelBase _currentViewModel;
        private bool _isDarkTheme = true;
        private string _accentColor = "#00ff88";
        private CultureInfo _currentCulture;

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (SetProperty(ref _isDarkTheme, value))
                {
                    UpdateThemeAsync().ConfigureAwait(false);
                }
            }
        }

        public string AccentColor
        {
            get => _accentColor;
            set
            {
                if (SetProperty(ref _accentColor, value))
                {
                    UpdateThemeAsync().ConfigureAwait(false);
                }
            }
        }

        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (SetProperty(ref _currentCulture, value))
                {
                    _localizationService.CurrentCulture = value;
                    OnPropertyChanged(nameof(AvailableCultures));
                }
            }
        }

        public List<CultureInfo> AvailableCultures { get; } = new List<CultureInfo>
        {
            new CultureInfo("en-US"),
            new CultureInfo("pl-PL")
        };

        public ICommand NavigateToDeploymentCommand { get; }
        public ICommand NavigateToServerSettingsCommand { get; }
        public ICommand NavigateToTerminalCommand { get; }
        public ICommand ChangeLanguageCommand { get; }

        public MainViewModel(
            ISettingsService settingsService,
            ILocalizationService localizationService,
            ServerSettingsViewModel serverSettingsViewModel,
            DeploymentViewModel deploymentViewModel,
            RemoteTerminalViewModel remoteTerminalViewModel)
        {
            _settingsService = settingsService;
            _localizationService = localizationService;
            _serverSettingsViewModel = serverSettingsViewModel;
            _deploymentViewModel = deploymentViewModel;
            _remoteTerminalViewModel = remoteTerminalViewModel;

            NavigateToDeploymentCommand = new RelayCommand(() => CurrentViewModel = _deploymentViewModel);
            NavigateToServerSettingsCommand = new RelayCommand(() => CurrentViewModel = _serverSettingsViewModel);
            NavigateToTerminalCommand = new RelayCommand(() => CurrentViewModel = _remoteTerminalViewModel);
            ChangeLanguageCommand = new RelayCommand<CultureInfo>(culture => CurrentCulture = culture);

            CurrentViewModel = _deploymentViewModel;
            _currentCulture = Thread.CurrentThread.CurrentUICulture;

            LoadThemeSettingsAsync().ConfigureAwait(false);
        }

        private async Task LoadThemeSettingsAsync()
        {
            var settings = await _settingsService.LoadSettingsAsync();
            _isDarkTheme = settings.IsDarkTheme;
            _accentColor = settings.AccentColor;

            ThemeManager.ApplyTheme(settings);

            OnPropertyChanged(nameof(IsDarkTheme));
            OnPropertyChanged(nameof(AccentColor));
        }

        private async Task UpdateThemeAsync()
        {
            var settings = await _settingsService.LoadSettingsAsync();
            settings.IsDarkTheme = IsDarkTheme;
            settings.AccentColor = AccentColor;

            await _settingsService.SaveSettingsAsync(settings);
            ThemeManager.ApplyTheme(settings);
        }
    }
}
