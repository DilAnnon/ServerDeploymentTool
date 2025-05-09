// Services/LocalizationService.cs
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Resources;

namespace ServerDeploymentTool.Services
{
    public interface ILocalizationService
    {
        CultureInfo CurrentCulture { get; set; }
        event EventHandler LanguageChanged;
        string GetString(string key);
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly ResourceManager _resourceManager;

        public LocalizationService()
        {
            _resourceManager = new ResourceManager(
                "ServerDeploymentTool.Properties.Resources",
                typeof(Properties.Resources).Assembly);
        }

        public event EventHandler LanguageChanged;

        public CultureInfo CurrentCulture
        {
            get => Thread.CurrentThread.CurrentUICulture;
            set
            {
                if (value != Thread.CurrentThread.CurrentUICulture)
                {
                    Thread.CurrentThread.CurrentUICulture = value;
                    Thread.CurrentThread.CurrentCulture = value;
                    LanguageChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string GetString(string key)
        {
            try
            {
                return _resourceManager.GetString(key, CurrentCulture) ?? key;
            }
            catch
            {
                return key;
            }
        }
    }
}
