// Themes/ThemeManager.cs
using MaterialDesignThemes.Wpf;
using ServerDeploymentTool.Models;
using System.Windows.Media;

namespace ServerDeploymentTool.Themes
{
    public static class ThemeManager
    {
        public static void ApplyTheme(AppSettings settings)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            theme.SetBaseTheme(settings.IsDarkTheme ? 
                Theme.Dark : 
                Theme.Light);

            if (ColorConverter.ConvertFromString(settings.AccentColor) is Color accentColor)
            {
                theme.SecondaryMid = accentColor;
            }

            paletteHelper.SetTheme(theme);
        }
    }
}
