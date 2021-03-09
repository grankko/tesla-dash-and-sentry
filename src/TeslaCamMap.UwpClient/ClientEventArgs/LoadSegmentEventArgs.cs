using System;
using TeslaCamMap.UwpClient.ViewModels;

namespace TeslaCamMap.UwpClient.ClientEventArgs
{
    public class LoadSegmentEventArgs : EventArgs
    {
        public EventSegmentViewModel Segment { get; set; }
        public LoadSegmentEventArgs(EventSegmentViewModel segment)
        {
            Segment = segment;
        }
    }
}
