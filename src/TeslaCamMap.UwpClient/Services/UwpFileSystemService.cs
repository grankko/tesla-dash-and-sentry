using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeslaCamMap.Lib.Model;
using TeslaCamMap.UwpClient.Model;
using TeslaCamMap.UwpClient.ClientEventArgs;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace TeslaCamMap.UwpClient.Services
{
    //todo: Parsing strategy should change to first locate all event.json files instead of traversing the folders.
    //todo: Separate parse logic from IO, make it testable.
    //todo: Error handling

    public class UwpFileSystemService
    {
        public event EventHandler<ProgressEventArgs> ProgressUpdated;

        private const string MediaDurationPropertyName = "System.Media.Duration";
        private const string VideoFrameRatePropertyName = "System.Video.FrameRate";
        private const string SavedClipsFolderName = "SavedClips";
        private const string SentryClipsFolderName = "SentryClips";
        private const string EventFileName = "event.json";
        private const string ThumbnailFileName = "thumb.png";

        /// <summary>
        /// Parse file metadata in Tesla Cam folders.
        /// </summary>
        /// <param name="folders">Tesla Cam folders</param>
        /// <returns>Model representing a list of TeslaEvents found in the selected folders.</returns>
        public async Task<List<UwpTeslaEvent>> ParseFiles(IReadOnlyList<StorageFolder> folders)
        {
            var result = new List<UwpTeslaEvent>();

            foreach (var folder in folders)   // "SavedClips", "SentryClips"
            {
                EventStoreLocation storeLocation = EventStoreLocation.Unkown;
                if (folder.Name.Equals(SavedClipsFolderName, StringComparison.InvariantCultureIgnoreCase))
                    storeLocation = EventStoreLocation.SavedClip;
                else if (folder.Name.Equals(SentryClipsFolderName, StringComparison.InvariantCultureIgnoreCase))
                    storeLocation = EventStoreLocation.SentryClip;

                var eventFolders = await folder.GetFoldersAsync();

                foreach (var eventFolder in eventFolders) // "2020-07-02_12-13-41" etc ..
                {
                    var eventMetadataFile = await eventFolder.GetFileAsync(EventFileName);
                    if (eventMetadataFile != null)
                    {
                        UwpTeslaEvent teslaEvent = await ParseTeslaEvent(storeLocation, eventFolder, eventMetadataFile);
                        result.Add(teslaEvent);
                        ProgressUpdated?.Invoke(this, new ProgressEventArgs(result.Count));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Parse all information needed for a single event.
        /// </summary>
        /// <param name="storeLocation">The Tesla Cam folder the event was located in.</param>
        /// <param name="eventFolder">Path the the event folder.</param>
        /// <param name="eventMetadataFile">Metadata file associated with the event.</param>
        private async Task<UwpTeslaEvent> ParseTeslaEvent(EventStoreLocation storeLocation, StorageFolder eventFolder, StorageFile eventMetadataFile)
        {
            var eventText = await FileIO.ReadTextAsync(eventMetadataFile);
            var metadata = System.Text.Json.JsonSerializer.Deserialize<TeslaEventJson>(eventText);
            var teslaEvent = new UwpTeslaEvent(metadata);
            teslaEvent.StoreLocation = storeLocation;
            teslaEvent.FolderPath = eventFolder.Path;

            var thumbnailFile = await eventFolder.GetFileAsync(ThumbnailFileName);
            teslaEvent.ThumbnailFile = thumbnailFile;
            if (thumbnailFile != null)
                teslaEvent.ThumbnailPath = thumbnailFile.Path;

            var eventFolderFiles = await eventFolder.GetFilesAsync();

            // Group all video files into segments based on the timestamp in the file name
            var results = eventFolderFiles.Where(f => f.FileType.Equals(".mp4")).GroupBy(
                f => ParseTimestamp(f.Name),
                f => f,
                (key, g) => new { SegmentTimestamp = key, ClipFiles = g.ToList() });


            foreach (var fileGroup in results.OrderBy(r => r.SegmentTimestamp))
            {
                var eventSegment = new EventSegment();
                eventSegment.SegmentTimestamp = fileGroup.SegmentTimestamp;
                
                eventSegment.Clips = new List<Clip>();
                foreach (var clipFile in fileGroup.ClipFiles)
                    eventSegment.Clips.Add(ParseClipFile(clipFile));

                teslaEvent.Segments.Add(eventSegment);
            }

            // Link event timestamps
            for (int i = 0; i <teslaEvent.Segments.Count-1; i++)
                teslaEvent.Segments[i].NextSegmentTimestamp = teslaEvent.Segments[i + 1].SegmentTimestamp;

            // Sets flag if a segment period contains the event timestamp
            var hotSegment = teslaEvent.Segments.Where(s => s.SegmentTimestamp < teslaEvent.Timestamp && (!s.NextSegmentTimestamp.HasValue || teslaEvent.Timestamp < s.NextSegmentTimestamp.Value)).FirstOrDefault();
            if (hotSegment != null)
                hotSegment.ContainsEventTimestamp = true;

            return teslaEvent;
        }

        public async Task<string> GetStringFromApplicationFile(string path)
        {
            var fullPath = $"ms-appx:///{path}";
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(fullPath));
            var result = await FileIO.ReadTextAsync(file);
            return result;
        }

        public async Task<BitmapImage> LoadImageFromStorageFile(IStorageFile imageFile)
        {
            BitmapImage bitmapImage = null;

            using (IRandomAccessStream fileStream =
                await imageFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);
            }

            return bitmapImage;
        }

        /// <summary>
        /// Reads video metadata properties from filesystem and populates properties in the event.
        /// </summary>
        /// <remarks>Costly operation. Don't do this for all events at the time of scanning through the top directory. Populate metadata only when user needs it.</remarks>
        public async Task<UwpTeslaEvent> PopulateEventMetadata(UwpTeslaEvent teslaEvent)
        {
            foreach (var segment in teslaEvent.Segments)
            {
                foreach (var clip in segment.Clips.Cast<UwpClip>())
                {
                    StorageFile storageFile = (StorageFile)clip.ClipFile;

                    IDictionary<string, object> retrieveProperties = await storageFile.Properties.RetrievePropertiesAsync(new string[] { VideoFrameRatePropertyName, MediaDurationPropertyName });
                    clip.FrameRate = ((uint)retrieveProperties[VideoFrameRatePropertyName]);

                    var duration = ((ulong)retrieveProperties[MediaDurationPropertyName]);
                    clip.Duration = TimeSpan.FromTicks((long)duration);
                }

                segment.MaxClipFrameDuration = segment.Clips.Max(c => (int)c.FrameDuration);
                segment.MaxClipDuration = segment.Clips.Max(c => c.Duration);
            }

            return teslaEvent;
        }

        /// <summary>
        /// Gets a model of each clip (camera angle) for a segment of the event.
        /// </summary>
        /// <param name="clipFile"></param>
        private UwpClip ParseClipFile(StorageFile clipFile)
        {
            var clip = new UwpClip();
            if (clipFile.Name.Contains("left", StringComparison.InvariantCultureIgnoreCase))
                clip.Camera = Camera.LeftRepeater;
            else if (clipFile.Name.Contains("front", StringComparison.InvariantCultureIgnoreCase))
                clip.Camera = Camera.Front;
            else if (clipFile.Name.Contains("right", StringComparison.InvariantCultureIgnoreCase))
                clip.Camera = Camera.RightRepeater;
            else if (clipFile.Name.Contains("back", StringComparison.InvariantCultureIgnoreCase))
                clip.Camera = Camera.Back;
            else
                clip.Camera = Camera.Unknown;

            clip.FilePath = clipFile.Path;
            clip.FileName = clipFile.Name;

            clip.ClipFile = clipFile;
            return clip;
        }
        private static DateTime ParseTimestamp(string fileName)
        {
            // todo: use regex instead
            //filname format: 2020-07-02_12-10-39-back.mp4

            int year = int.Parse(fileName.Substring(0, 4));
            int month = int.Parse(fileName.Substring(5, 2));
            int day = int.Parse(fileName.Substring(8, 2));
            int hour = int.Parse(fileName.Substring(11, 2));
            int minute = int.Parse(fileName.Substring(14, 2));
            int second = int.Parse(fileName.Substring(17, 2));

            return new DateTime(year, month, day, hour, minute, second);
        }
    }
}
