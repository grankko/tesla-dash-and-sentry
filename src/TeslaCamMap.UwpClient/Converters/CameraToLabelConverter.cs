using System;
using TeslaCamMap.UwpClient.Model;
using Windows.UI.Xaml.Data;

namespace TeslaCamMap.UwpClient.Converters
{
    public class CameraToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Camera)
            {
                switch ((Camera)value) {
                    case Camera.LeftRepeater:
                        return "Left repeater";
                    case Camera.Front:
                        return "Front";
                    case Camera.RightRepeater:
                        return "Right repeater";
                    case Camera.Back:
                        return "Back";
                    default:
                        return "Unknown camera";
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
