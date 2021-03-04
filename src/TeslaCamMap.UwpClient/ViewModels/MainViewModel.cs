using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeslaCamMap.Lib.Model;
using TeslaCamMap.UwpClient.Commands;
using TeslaCamMap.UwpClient.Model;
using TeslaCamMap.UwpClient.Services;
using Windows.Devices.Geolocation;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace TeslaCamMap.UwpClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const int DefaultZoomLevel = 15;


        private FileSystemService _fileSystemService;

        private int _mapZoom;
        public int MapZoom
        {
            get { return _mapZoom; }
            set
            {
                _mapZoom = value;
                OnPropertyChanged();
            }
        }

        private Geopoint _mapCenter;
        public Geopoint MapCenter
        {
            get { return _mapCenter; }
            set
            {
                _mapCenter = value;
                OnPropertyChanged();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set {
                _isBusy = value;
                OnPropertyChanged();
                PickFolderCommand.RaiseCanExecuteChanged();
            }
        }

        private string _bingMapServiceToken;
        public string BingMapServiceToken
        {
            get { return _bingMapServiceToken; }
            set
            {
                _bingMapServiceToken = value;
                OnPropertyChanged();
            }
        }

        private string _selectedFolderLabelText;
        public string SelectedFolderLabelText
        {
            get { return _selectedFolderLabelText; }
            set
            {
                _selectedFolderLabelText = value;
                OnPropertyChanged();
            }
        }

        private UwpTeslaEvent _selectedTeslaEvent;
        public UwpTeslaEvent SelectedTeslaEvent
        {
            get { return _selectedTeslaEvent; }
            set
            {
                _selectedTeslaEvent = value;
                OnPropertyChanged();

                if (MapZoom < DefaultZoomLevel)
                    MapZoom = DefaultZoomLevel;

                MapCenter = _selectedTeslaEvent.EventMapIcon.Location;
            }
        }

        private BitmapImage _selectedTeslaEventThumb;
        public BitmapImage SelectedTeslaEventThumb
        {
            get { return _selectedTeslaEventThumb; }
            set
            {
                _selectedTeslaEventThumb = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<MapLayer> _teslaEventMapLayer;
        public ObservableCollection<MapLayer> TeslaEventMapLayer
        {
            get { return _teslaEventMapLayer; }
            set
            {
                _teslaEventMapLayer = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<UwpTeslaEvent> _teslaEvents;
        public ObservableCollection<UwpTeslaEvent> TeslaEvents
        {
            get { return _teslaEvents; }
            set
            {
                _teslaEvents = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand PickFolderCommand { get; set; }
        public RelayCommand ViewVideoCommand { get; set; }

        public MainViewModel()
        {
            _fileSystemService = new FileSystemService();
            PickFolderCommand = new RelayCommand(PickFolderCommandExecute, CanPickFolderCommandExecute);
            ViewVideoCommand = new RelayCommand(ViewVideoCommandExecute, CanViewVideoCommandExecute);

            this.PropertyChanged += MainViewModel_PropertyChanged;
        }

        private bool CanViewVideoCommandExecute(object arg)
        {
            return SelectedTeslaEvent != null;
        }

        private void ViewVideoCommandExecute(object obj)
        {
            ViewFrame.Navigate(typeof(EventDetailsPage), obj);
        }

        private bool CanPickFolderCommandExecute(object arg)
        {
            return !IsBusy;
        }

        // todo: this is a hack
        private async void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedTeslaEvent))
                SelectedTeslaEventThumb = await _fileSystemService.LoadImageFromStorageFile(SelectedTeslaEvent.ThumbnailFile);
        }

        //todo: hack
        public void OnMapElementClicked(MapElement clickedItem)
        {
            var teslaEvent = (UwpTeslaEvent)clickedItem.Tag;
            SelectedTeslaEvent = teslaEvent;
        }

        //todo: hack
        public async void OnLoaded()
        {
            BingMapServiceToken = await _fileSystemService.GetStringFromApplicationFile("bing_key");
            SelectedFolderLabelText = "No folder selected";
        }

        private async void PickFolderCommandExecute(object obj)
        {
            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".json");
            picker.FileTypeFilter.Add(".png");
            
            var result = await picker.PickSingleFolderAsync();

            if (result != null)
            {
                IsBusy = true;
                var folders = await result.GetFoldersAsync();

                var events = await _fileSystemService.ParseFiles(folders);
                TeslaEvents = new ObservableCollection<UwpTeslaEvent>(events);

                SelectedFolderLabelText = $"{result.Path} - {TeslaEvents.Count} events found.";

                //todo: databind and do this stuff in a IValueConverter instead?
                TeslaEventMapLayer = new ObservableCollection<MapLayer>();
                var layer = new MapElementsLayer();
                foreach (var teslaEvent in events)
                {
                    MapIcon eventMapIcon = new MapIcon();
                    eventMapIcon.Location = new Geopoint(new BasicGeoposition() { Latitude = teslaEvent.EstimatedLatitude, Longitude = teslaEvent.EstimatedLongitude });
                    eventMapIcon.Tag = teslaEvent;
                    layer.MapElements.Add(eventMapIcon);

                    teslaEvent.EventMapIcon = eventMapIcon;
                }
                TeslaEventMapLayer.Add(layer);
            }

            IsBusy = false;
        }
    }
}
