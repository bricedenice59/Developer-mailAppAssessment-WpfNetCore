using System;
using System.Windows.Data;
using System.Globalization;
using DeveloperTest.Helpers.Models;

namespace DeveloperTest.Utils.WPF.Converters;

[ValueConversion(typeof(Enum), typeof(string))]
public class EncryptionTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return null;
        if (Enum.TryParse(value.ToString(), out EncryptionTypes result))
        {
            return result switch
            {
                EncryptionTypes.Unencrypted => "Unencrypted",
                EncryptionTypes.SSLTLS => "SSLTLS",
                EncryptionTypes.STARTTLS => "STARTTLS",
                _ => throw new NotImplementedException($"No string convertion available for encryption {result}"),
            };
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return null;
        if (Enum.TryParse(value.ToString(), out EncryptionTypes result))
            return result;

        return null;
    }
}
