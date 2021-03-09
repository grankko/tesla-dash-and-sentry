using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TeslaCamMap.Lib.Model;
using TeslaCamMap.UwpClient.Model;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace TeslaCamMap.UwpClient.Services
{
    //todo: Parsing strategy should change to first locate all event.json files instead of traversing the folders.
    //todo: Separate parse logic from IO, make it testable.
    //todo: Error handling

    public class UwpFileSystemService
    {
        private const string MediaDuratioPropertyName = "System.Media.Duration";
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
                f => ParseTimeStamp(f.Name),
                f => f,
                (key, g) => new { SegmentTimestamp = key, ClipFiles = g.ToList() });


            foreach (var fileGroup in results)
            {
                var eventSegment = new EventSegment();
                eventSegment.SegmentTimestamp = fileGroup.SegmentTimestamp;
                
                eventSegment.Clips = new List<Clip>();
                foreach (var clipFile in fileGroup.ClipFiles)
                    eventSegment.Clips.Add(await ParseClipFile(clipFile));

                eventSegment.MaxClipFrameDuration = eventSegment.Clips.Max(c => (int)c.FrameDuration);
                eventSegment.MaxClipDuration = eventSegment.Clips.Max(c => c.Duration);

                teslaEvent.Segments.Add(eventSegment);
            }

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
        /// Gets a model of each clip (camera angle) for a segment of the event.
        /// </summary>
        /// <param name="clipFile"></param>
        private async Task<UwpClip> ParseClipFile(StorageFile clipFile)
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

            IDictionary<string, object> retrieveProperties = await clipFile.Properties.RetrievePropertiesAsync(new string[] { VideoFrameRatePropertyName, "System.Media.Duration" });
            clip.FrameRate = ((uint)retrieveProperties[VideoFrameRatePropertyName]);
            
            var duration = ((ulong)retrieveProperties[MediaDuratioPropertyName]);
            clip.Duration = TimeSpan.FromTicks((long)duration);

            clip.FilePath = clipFile.Path;
            clip.FileName = clipFile.Name;

            clip.ClipFile = clipFile;
            return clip;
        }
        private static DateTime ParseTimeStamp(string fileName)
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
