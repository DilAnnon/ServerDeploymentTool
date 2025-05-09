// Services/SettingsService.cs
using Newtonsoft.Json;
using ServerDeploymentTool.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ServerDeploymentTool.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ServerDeploymentTool");

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            _settingsFilePath = Path.Combine(appDataPath, "settings.json");
        }

        public async Task<AppSettings> LoadSettingsAsync()
        {
            if (!File.Exists(_settingsFilePath))
                return new AppSettings();

            try
            {
                string json = await File.ReadAllTextAsync(_settingsFilePath);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            catch (Exception)
            {
                return new AppSettings();
            }
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
    }
}
