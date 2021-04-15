using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZazBoy
{
    class RelativeHeightScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double scaleFactor = Double.Parse(parameter.ToString());
            double referenceControlHeight = (double)value;
            double targetControlHeight = referenceControlHeight * scaleFactor;
            return targetControlHeight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double scaleFactor = Double.Parse(parameter.ToString());
            double inverseScale = 1.0f / scaleFactor;
            double referenceControlHeight = (double)value;
            double targetControlHeight = referenceControlHeight * inverseScale;
            return targetControlHeight;
        }
    }
}
