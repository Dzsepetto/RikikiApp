using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RikikiApp.Converters
{
    public class GameStatusIconConverter : IValueConverter
    {
        public object Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            return value?.ToString()?.ToLowerInvariant() switch
            {
                "setup" => "♠",
                "inprogress" or "in progress" => "♦",
                "finished" => "♥",
                _ => "♣"
            };
        }

        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
