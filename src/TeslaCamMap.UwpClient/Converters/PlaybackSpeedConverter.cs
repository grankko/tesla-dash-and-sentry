using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace TeslaCamMap.UwpClient.Converters
{
    public class PlaybackSpeedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int currentSpeed, controllerSpeedValue;
            if (parameter != null)
            {
                currentSpeed = (int)value;
                controllerSpeedValue = int.Parse((string)parameter);
                if (currentSpeed == controllerSpeedValue)
                    return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return int.Parse((string)parameter);
        }
    }
}
