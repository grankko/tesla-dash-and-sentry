using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeslaCamMap.Lib.Model;
using Windows.UI.Xaml.Data;

namespace TeslaCamMap.UwpClient.Converters
{
    public class EventReasonLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is EventReason)
            {
                switch (value)
                {
                    case EventReason.SentryAwareObjectDetection:
                        return "Sentry Aware object detection";
                    case EventReason.UserInteractionDashCamTapped:
                        return "Saved by user";
                    case EventReason.UserInteractionHonk:
                        return "User honked";
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
