using System;
using System.Globalization;
using System.Windows.Data;

namespace MultiPingEnumerator
{
    public class PacketConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double val)
            {
                // If slider is at the max (21), return "Continuous"
                if (val >= 21)
                    return "∞ Continuous";

                return val.ToString("0");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}