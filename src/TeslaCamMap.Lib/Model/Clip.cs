using System;

namespace TeslaCamMap.Lib.Model
{
    /// <summary>
    /// Represents one video clip of a segment of the event.
    /// </summary>
    public class Clip
    {
        public Camera Camera { get; set; }
        public string FilePath { get; set; }
        public uint FrameRate { get; set; }
        public uint FrameDuration { get => FrameRate / 1000; }
        public TimeSpan Duration { get; set; }
        public string FileName { get; set; }
    }
}