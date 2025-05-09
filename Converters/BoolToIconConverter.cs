// Converters/BoolToIconConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace ServerDeploymentTool.Converters
{
    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string paramString)
            {
                var parts = paramString.Split(',');
                if (parts.Length == 2)
                {
                    var trueValue = parts[0];
                    var falseValue = parts[1];

                    return Enum.TryParse<PackIconKind>(boolValue ? trueValue : falseValue, out var result)
                        ? result
                        : PackIconKind.QuestionMark;
                }
            }
            return PackIconKind.QuestionMark;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
