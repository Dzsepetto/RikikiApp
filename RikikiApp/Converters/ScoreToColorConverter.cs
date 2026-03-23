using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RikikiApp.Converters
{
    public class ScoreToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Colors.Gray;

            int score = (int)value;

            if (score > 0)
                return Colors.Green;

            if (score < 0)
                return Colors.Red;

            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
