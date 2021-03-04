using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeslaCamMap.Lib.Model;
using Windows.Storage;

namespace TeslaCamMap.UwpClient.Model
{
    public class UwpTeslaEvent : TeslaEvent
    {
        public UwpTeslaEvent(TeslaEventJson metadata) : base(metadata)
        {
        }

        public IStorageFile ThumbnailFile { get; set; }
    }
}
