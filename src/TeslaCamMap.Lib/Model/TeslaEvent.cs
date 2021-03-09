using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TeslaCamMap.Lib.Model
{
    /// <summary>
    /// Represents one unique event.
    /// </summary>
    public class TeslaEvent
    {
        public DateTime TimeStamp { get; set; }
        public string City { get; set; }
        public double EstimatedLatitude { get; set; }
        public double EstimatedLongitude { get; set; }
        public EventReason Reason { get; set; }
        public EventStoreLocation StoreLocation { get; set; }
        public string FolderPath { get; set; }
        
        /// <summary>
        /// Each event consists of multiple segments with video from all active cameras.
        /// </summary>
        public List<EventSegment> Segments { get; set; }
        public string ThumbnailPath { get; set; }

        public TeslaEvent(TeslaEventJson metadata)
        {
            Segments = new List<EventSegment>();
            TimeStamp = metadata.timestamp;
            City = metadata.city;
            EstimatedLatitude = double.Parse(metadata.est_lat, CultureInfo.InvariantCulture);
            EstimatedLongitude = double.Parse(metadata.est_lon, CultureInfo.InvariantCulture);
            Reason = EventReason.Unknown;

            if (metadata.reason.Equals("sentry_aware_object_detection", StringComparison.InvariantCultureIgnoreCase))
                Reason = EventReason.SentryAwareObjectDetection;
            else if (metadata.reason.Equals("user_interaction_dashcam_icon_tapped", StringComparison.InvariantCultureIgnoreCase))
                Reason = EventReason.UserInteractionDashCamTapped;
            else if (metadata.reason.Equals("user_interaction_honk", StringComparison.InvariantCultureIgnoreCase))
                Reason = EventReason.UserInteractionHonk;
        }
    }
}