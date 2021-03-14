using System.Collections.Generic;
using TeslaCamMap.UwpClient.Model;

namespace TeslaCamMap.UwpClient.Services
{
    public class FileSerivceParseResult
    {
        public List<TeslaEvent> Result { get; set; }
        public string ParsedPath { get; set; }

    }
}
