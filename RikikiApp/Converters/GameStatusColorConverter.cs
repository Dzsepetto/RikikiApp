using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RikikiApp.Converters
{
    public class GameStatusColorConverter : IValueConverter
    {
        public object Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            return value?.ToString()?.ToLowerInvariant() switch
            {
                "setup" => Color.FromArgb("#F4B544"),
                "inprogress" or "in progress" => Color.FromArgb("#E97931"),
                "finished" => Color.FromArgb("#E83E59"),
                _ => Color.FromArgb("#D9E8DF")
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
