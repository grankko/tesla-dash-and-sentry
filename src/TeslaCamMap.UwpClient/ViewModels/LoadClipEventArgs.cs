using System;

namespace TeslaCamMap.UwpClient.ViewModels
{
    public class LoadClipEventArgs : EventArgs
    {
        public ClipViewModel Clip { get; set; }
        public LoadClipEventArgs(ClipViewModel clip)
        {
            Clip = clip;
        }
    }
}
