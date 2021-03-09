using System;
using System.Collections.Generic;
using TeslaCamMap.Lib.Model;
using TeslaCamMap.UwpClient.Model;

namespace TeslaCamMap.UwpClient.ViewModels
{
    public class EventSegmentViewModel : ViewModelBase
    {
        public EventSegment Model { get; set; }
        public EventSegmentViewModel(EventSegment model)
        {
            Model = model;
        }

        public int SegmentIndex { get; set; }
    }
}
