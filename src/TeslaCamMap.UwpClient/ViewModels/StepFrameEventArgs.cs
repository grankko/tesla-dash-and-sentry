using System;

namespace TeslaCamMap.UwpClient.ViewModels
{
    public class StepFrameEventArgs : EventArgs
    {
        public bool StepForward { get; set; }

        public StepFrameEventArgs(bool stepForward)
        {
            StepForward = stepForward;
        }

    }
}