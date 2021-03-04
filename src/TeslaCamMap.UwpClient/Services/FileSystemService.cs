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
    public class FileSystemService
    {
        private readonly string _savedClipsFolderName = "SavedClips";
        private readonly string _sentryClipsFolderName = "SentryClips";
        private readonly string _eventFileName = "event.json";
        private readonly string _thumbnailFileName = "thumb.png";

        public async Task<List<UwpTeslaEvent>> ParseFiles(IReadOnlyList<StorageFolder> folders)
        {
            var result = new List<UwpTeslaEvent>();

            foreach (var folder in folders)
            {
                EventStoreLocation storeLocation = EventStoreLocation.Unkown;
                if (folder.Name.Equals(_savedClipsFolderName, StringComparison.InvariantCultureIgnoreCase))
                    storeLocation = EventStoreLocation.SavedClip;
                else if (folder.Name.Equals(_sentryClipsFolderName, StringComparison.InvariantCultureIgnoreCase))
                    storeLocation = EventStoreLocation.SentryClip;

                var eventFolders = await folder.GetFoldersAsync();

                foreach (var eventFolder in eventFolders)
                {
                    var eventMetadataFile = await eventFolder.GetFileAsync(_eventFileName);
                    if (eventMetadataFile != null)
                    {
                        UwpTeslaEvent teslaEvent = await ParseTeslaEvent(storeLocation, eventFolder, eventMetadataFile);
                        result.Add(teslaEvent);
                    }
                }
            }

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

        private async Task<UwpTeslaEvent> ParseTeslaEvent(EventStoreLocation storeLocation, StorageFolder eventFolder, StorageFile eventMetadataFile)
        {
            var eventText = await FileIO.ReadTextAsync(eventMetadataFile);
            var metadata = System.Text.Json.JsonSerializer.Deserialize<TeslaEventJson>(eventText);
            var teslaEvent = new UwpTeslaEvent(metadata);
            teslaEvent.StoreLocation = storeLocation;
            teslaEvent.FolderPath = eventFolder.Path;

            var thumbnailFile = await eventFolder.GetFileAsync(_thumbnailFileName);
            teslaEvent.ThumbnailFile = thumbnailFile;
            if (thumbnailFile != null)
                teslaEvent.ThumbnailPath = thumbnailFile.Path;

            var eventFolderFiles = await eventFolder.GetFilesAsync();

            foreach (var eventFolderFile in eventFolderFiles)
                if (eventFolderFile.FileType.Equals(".mp4", StringComparison.InvariantCultureIgnoreCase))
                    teslaEvent.Clips.Add(ParseClipFile(eventFolderFile));

            return teslaEvent;
        }

        private static UwpClip ParseClipFile(StorageFile clipFile
            )
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
    }
}
