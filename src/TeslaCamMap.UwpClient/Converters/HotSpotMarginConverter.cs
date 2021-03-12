using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace TeslaCamMap.UwpClient.Converters
{
    public class HotSpotMarginConverter : IValueConverter
    {
        private const int HotSpotIconRadius = 5;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double? && ((double?)value).HasValue)
            {
                return new Thickness(((double?)value).Value - HotSpotIconRadius, 0, 0, 0);
            }

            return new Thickness(0, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
