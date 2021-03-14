using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TeslaCamMap.Lib.Model;
using TeslaCamMap.UwpClient.ClientEventArgs;
using TeslaCamMap.UwpClient.Model;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace TeslaCamMap.UwpClient.Services
{
    public class ParseResult
    {
        public List<UwpTeslaEvent> Result { get; set; }
        public string ParsedPath { get; set; }

    }

    public class UwpFileSystemService
    {
        public event EventHandler<ProgressEventArgs> ProgressUpdated;

        private const string EventMetadataFileExtension = ".json";
        private const string EventVideoFileExtension = ".mp4";
        private const string EventThumbnailFileExtension = ".png";

        private const string SavedClipsFolderName = "SavedClips";
        private const string SentryClipsFolderName = "SentryClips";

        private const string MediaDurationPropertyName = "System.Media.Duration";
        private const string VideoFrameRatePropertyName = "System.Video.FrameRate";

        private Regex _eventFolderNameRegex = new Regex(@"(?<EventFolderName>[\d]{4}-[\d]{2}-[\d]{2}_[\d]{2}-[\d]{2}-[\d]{2})");

        public async Task<ParseResult> OpenAndParseFolder()
        {
            var result = new ParseResult();

            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add(EventVideoFileExtension);
            picker.FileTypeFilter.Add(EventMetadataFileExtension);
            picker.FileTypeFilter.Add(EventThumbnailFileExtension);

            var folderResult = await picker.PickSingleFolderAsync();
            if (folderResult != null)
            {
                var files = await folderResult.GetFilesAsync(CommonFileQuery.OrderByName);
                result.Result = await ParseFiles(files);
                result.ParsedPath = folderResult.Path;
            }

            return result;
        }

        public async Task<List<UwpTeslaEvent>> ParseFiles(IReadOnlyList<StorageFile> files)
        {
            var result = new List<UwpTeslaEvent>();

            var eventMetadataFiles = files.Where(f => f.FileType.Equals(EventMetadataFileExtension));
            var videoFiles = files.Where(f => f.FileType.Equals(EventVideoFileExtension));
            var thumbnailFiles = files.Where(f => f.FileType.Equals(EventThumbnailFileExtension));

            foreach (var metadataFile in eventMetadataFiles)
            {
                var folderName = _eventFolderNameRegex.Match(metadataFile.Path).Groups["EventFolderName"].Value;

                EventStoreLocation storeLocation = EventStoreLocation.Unkown;
                if (metadataFile.Path.Contains(SavedClipsFolderName, StringComparison.InvariantCultureIgnoreCase))
                    storeLocation = EventStoreLocation.SavedClip;
                else if (metadataFile.Path.Contains(SentryClipsFolderName, StringComparison.InvariantCultureIgnoreCase))
                    storeLocation = EventStoreLocation.SentryClip;

                var eventText = await FileIO.ReadTextAsync(metadataFile);
                var metadata = JsonSerializer.Deserialize<TeslaEventJson>(eventText);

                var teslaEvent = new UwpTeslaEvent(metadata);
                teslaEvent.StoreLocation = storeLocation;
                teslaEvent.FolderPath = metadataFile.Path;

                var thumbnailFile = files.First(f => f.FileType.Equals(EventThumbnailFileExtension) && f.Path.Contains(folderName));
                teslaEvent.ThumbnailFile = thumbnailFile;
                teslaEvent.ThumbnailPath = thumbnailFile.Path;

                var results = files.Where(f => f.Path.Contains(folderName) && f.FileType.Equals(EventVideoFileExtension)).GroupBy(
                    f => ParseTimestampFromFilename(f.Name),
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
                for (int i = 0; i < teslaEvent.Segments.Count - 1; i++)
                    teslaEvent.Segments[i].NextSegmentTimestamp = teslaEvent.Segments[i + 1].SegmentTimestamp;

                // Sets flag if a segment period contains the event timestamp
                var hotSegment = teslaEvent.Segments.Where(s => s.SegmentTimestamp < teslaEvent.Timestamp && (!s.NextSegmentTimestamp.HasValue || teslaEvent.Timestamp < s.NextSegmentTimestamp.Value)).FirstOrDefault();
                if (hotSegment != null)
                    hotSegment.ContainsEventTimestamp = true;

                result.Add(teslaEvent);
                ProgressUpdated?.Invoke(this, new ProgressEventArgs(result.Count));
            }

            return result.OrderBy(r => r.Timestamp).ToList();
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

        public static DateTime ParseTimestampFromFilename(string fileName)
        {
            //filname format: 2020-07-02_12-10-39-back.mp4
            var dateRegex = new Regex(@"^(?<year>[\d]{4})-(?<month>[\d]{2})-(?<day>[\d]{2})_(?<hour>[\d]{2})-(?<minute>[\d]{2})-(?<second>[\d]{2})");
            var match = dateRegex.Match(fileName);

            int year = int.Parse(match.Groups["year"].Value);
            int month = int.Parse(match.Groups["month"].Value);
            int day = int.Parse(match.Groups["day"].Value);
            int hour = int.Parse(match.Groups["hour"].Value);
            int minute = int.Parse(match.Groups["minute"].Value);
            int second = int.Parse(match.Groups["second"].Value);

            return new DateTime(year, month, day, hour, minute, second);
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
    }
}
