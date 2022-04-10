using System;

namespace TeslaCamMap.UwpClient.ClientEventArgs
{
    public class PlaybackSpeedChangedEventArgs : EventArgs
    {
        public int NewPlaybackSpeed { get; set; }

        public PlaybackSpeedChangedEventArgs(int newPlaybackSpeed)
        {
            NewPlaybackSpeed = newPlaybackSpeed;
        }
    }
}