using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeslaCamMap.Lib.Model;
using Windows.UI.Xaml.Data;

namespace TeslaCamMap.UwpClient.Converters
{
    public class EventReasonToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is EventReason)
            {
                var reason = (EventReason)value;
                switch (reason)
                {
                    case EventReason.SentryAwareObjectDetection:
                        return "\xE8B8";
                    case EventReason.UserInteractionDashCamTapped:
                        return "\xE78C";
                    case EventReason.UserInteractionHonk:
                        return "\xF0EE";
                    default:
                        return "\xF142";

                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
