using System;
using TeslaCamMap.UwpClient.ViewModels;

namespace TeslaCamMap.UwpClient.ClientEventArgs
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
