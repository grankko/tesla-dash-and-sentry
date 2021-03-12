using System;
using System.Collections.Generic;
using TeslaCamMap.Lib.Model;
using TeslaCamMap.UwpClient.Model;

namespace TeslaCamMap.UwpClient.ViewModels
{
    public class EventSegmentViewModel : ViewModelBase
    {
        public EventSegment Model { get; set; }

        private DateTime _eventHotSpot;
        private double? _hotSpotVideoPositionAsMargin;
        public double? HotSpotVideoPositionAsMargin
        {
            get => _hotSpotVideoPositionAsMargin;
            set
            {
                _hotSpotVideoPositionAsMargin = value;
                OnPropertyChanged();
            }
        }
        public int SegmentIndex { get; set; }
        public EventSegmentViewModel(EventSegment model, DateTime eventHotSpot)
        {
            Model = model;
            _eventHotSpot = eventHotSpot;
        }

        public void CalculateHotspot(double sliderWidth)
        {
            if (Model.ContainsEventTimestamp)
            {
                var hotSpotTimeSpanFromStart = _eventHotSpot - Model.SegmentTimestamp;
                var percentage = hotSpotTimeSpanFromStart.TotalMilliseconds / Model.MaxClipDuration.TotalMilliseconds;
                
                if (percentage > 1)
                    percentage = 1;

                HotSpotVideoPositionAsMargin = sliderWidth * percentage;
            }
        }
    }
}
