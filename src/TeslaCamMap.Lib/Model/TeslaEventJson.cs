using System;

namespace TeslaCamMap.Lib.Model
{
    public class TeslaEventJson
    {
        public DateTime timestamp { get; set; }
        public string city { get; set; }
        public string est_lat { get; set; }
        public string est_lon { get; set; }
        public string reason { get; set; }
    }
}