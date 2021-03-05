using System;

namespace TeslaCamMap.Lib.Model
{
    /// <summary>
    /// Represents one video clip of a segment of the event.
    /// </summary>
    public class Clip
    {
        private string _fileName;

        public Camera Camera { get; set; }
        public string FilePath { get; set; }
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                ParseTimeStamp(_fileName);
            }
        }

        public DateTime TimeStampFromFileName { get; set; }

        private void ParseTimeStamp(string fileName)
        {
            // todo: regex instead
            // todo2: not even used right now

            //filname format: 2020-07-02_12-10-39-back.mp4

            int year = int.Parse(fileName.Substring(0, 4));
            int month = int.Parse(fileName.Substring(5, 2));
            int day = int.Parse(fileName.Substring(8,2));
            int hour = int.Parse(fileName.Substring(11, 2));
            int minute = int.Parse(fileName.Substring(14, 2));
            int second = int.Parse(fileName.Substring(17, 2));

            TimeStampFromFileName = new DateTime(year, month, day, hour, minute, second);
        }
    }
}