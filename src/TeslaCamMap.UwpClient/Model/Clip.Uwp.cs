using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeslaCamMap.Lib.Model;
using Windows.Storage;

namespace TeslaCamMap.UwpClient.Model
{
    public class UwpClip : Clip
    {
        public IStorageFile ClipFile { get; set; }
    }
}
