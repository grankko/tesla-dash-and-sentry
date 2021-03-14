using System;
using System.Collections.Generic;
using System.Text;

namespace TeslaCamMap.UwpClient.Model
{
    public class EventSegment
    {
        /// <summary>
        /// Video files from each camera for one segment of an event.
        /// </summary>
        public List<Clip> Clips { get; set; }

        /// <summary>
        /// Timestamp derived from the filenames of the clips.
        /// </summary>
        public DateTime SegmentTimestamp { get; set; }

        /// <summary>
        /// Timestamp for the next segment, if available.
        /// </summary>
        public DateTime? NextSegmentTimestamp { get; set; }

        /// <summary>
        /// Clips can have different frame rates within the same segment. This property holds the value of the calculated maximum frame duration in milliseconds for the clips.
        /// </summary>
        public int? MaxClipFrameDuration { get; set; }
        public TimeSpan? MaxClipDuration { get; set; }
        public bool ContainsEventTimestamp { get; set; }
    }
}
