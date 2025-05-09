// Models/AppSettings.cs
using System;
using System.Windows.Media;

namespace ServerDeploymentTool.Models
{
    public class AppSettings
    {
        public bool IsDarkTheme { get; set; } = true;
        public string AccentColor { get; set; } = "#00ff88";
        public string PrimaryColor { get; set; } = "#212121";
        public ServerSettings ServerSettings { get; set; } = new ServerSettings();
        public string LastLocalFolderPath { get; set; } = string.Empty;
        public string Language { get; set; } = "pl-PL";
    }
}
