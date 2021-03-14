using System;
using TeslaCamMap.UwpClient.Model;
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
                        return "Sentry aware object detection";
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
