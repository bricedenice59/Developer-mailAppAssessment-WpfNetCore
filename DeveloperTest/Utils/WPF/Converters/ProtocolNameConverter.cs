using System;
using System.Windows.Data;
using System.Globalization;
using DeveloperTest.Helpers.Models;

namespace DeveloperTest.Utils.WPF.Converters;

[ValueConversion(typeof(Enum), typeof(string))]
public class ProtocolNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return null;
        if (Enum.TryParse(value.ToString(), out Protocols result))
        {
            return result switch
            {
                Protocols.IMAP => "IMAP",
                Protocols.POP3 => "POP3",
                _ => throw new NotImplementedException($"No string convertion available for protocol {result}"),
            };
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return null;
        if (Enum.TryParse(value.ToString(), out Protocols result))
            return result;

        return null;
    }
}
