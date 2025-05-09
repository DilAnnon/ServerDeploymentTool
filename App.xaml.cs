// App.xaml.cs
using Microsoft.Extensions.DependencyInjection;
using ServerDeploymentTool.Services;
using ServerDeploymentTool.ViewModels;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace ServerDeploymentTool
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public App()
        {
            // Add global exception handling
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"An unhandled exception occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // App.xaml.cs
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Ustaw domyślną kulturę na polski
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("pl-PL");
                Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");

                base.OnStartup(e);

                var services = new ServiceCollection();
                ConfigureServices(services);

                ServiceProvider = services.BuildServiceProvider();

                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas uruchamiania: {ex.Message}",
                               "Błąd uruchamiania",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
                Shutdown();
            }
        }



        private void ConfigureServices(IServiceCollection services)
        {
            try
            {
                // Register services
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<ISshService, SshService>();
                services.AddSingleton<IFileComparisonService, FileComparisonService>();

                // Register view models
                services.AddSingleton<ServerSettingsViewModel>();
                services.AddSingleton<DeploymentViewModel>();
                services.AddSingleton<RemoteTerminalViewModel>();
                services.AddSingleton<MainViewModel>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd konfiguracji serwisów: {ex.Message}",
                               "Błąd konfiguracji",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
                throw;
            }
        }
    }
}
