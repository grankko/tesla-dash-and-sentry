using System.Collections.Generic;
using TeslaCamMap.UwpClient.Model;

namespace TeslaCamMap.UwpClient.ViewModels
{
    public class ClipViewModel : ViewModelBase
    {
        public ClipViewModel()
        {
            Clips = new List<UwpClip>();
        }

        public List<UwpClip> Clips { get; set; }
        public string FileName { get; set; }
        public int ClipIndex { get; set; }
    }
}
