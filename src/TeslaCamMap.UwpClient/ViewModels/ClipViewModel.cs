using System;
using System.Collections.Generic;
using TeslaCamMap.UwpClient.Model;

namespace TeslaCamMap.UwpClient.ViewModels
{
    /// <summary>
    /// ViewModel representation of a Clip. The ViewModel of a clip contains all four videos of an event segment.
    /// </summary>
    public class ClipViewModel : ViewModelBase
    {
        public ClipViewModel()
        {
            Clips = new List<UwpClip>();
        }

        public List<UwpClip> Clips { get; set; }
        public string CommonFileNameSegment { get; set; }
        public DateTime TimeStamp { get; set; }
        public uint EstimatedFrameDuration { get; set; }
        public int ClipIndex { get; set; }
    }
}
