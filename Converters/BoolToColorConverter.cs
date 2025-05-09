// Converters/BoolToColorConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ServerDeploymentTool.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string paramString)
            {
                var parts = paramString.Split(',');
                if (parts.Length == 2)
                {
                    try
                    {
                        var converter = new BrushConverter();
                        return (Brush)converter.ConvertFromString(boolValue ? parts[0] : parts[1]);
                    }
                    catch
                    {
                        return Brushes.Gray;
                    }
                }
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
