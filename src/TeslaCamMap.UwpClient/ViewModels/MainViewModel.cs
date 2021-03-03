using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeslaCamMap.Lib.Model;
using TeslaCamMap.UwpClient.Commands;
using TeslaCamMap.UwpClient.Services;
using Windows.Devices.Geolocation;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI.Xaml.Controls.Maps;

namespace TeslaCamMap.UwpClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private FileSystemService _fileSystemService;

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

        private TeslaEvent _selectedTeslaEvent;
        public TeslaEvent SelectedTeslaEvent
        {
            get { return _selectedTeslaEvent; }
            set
            {
                _selectedTeslaEvent = value;
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

        public RelayCommand PickFolderCommand { get; set; }

        public MainViewModel()
        {
            _fileSystemService = new FileSystemService();
            PickFolderCommand = new RelayCommand(PickFolderCommandExecute, CanPickFolderCommandExecute);
        }

        private bool CanPickFolderCommandExecute(object arg)
        {
            return !IsBusy;
        }

        internal void OnMapElementClicked(MapElement clickedItem)
        {
            SelectedTeslaEvent = (TeslaEvent)clickedItem.Tag;
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
                var files = await result.GetFilesAsync(CommonFileQuery.OrderByName);
                var folders = await result.GetFoldersAsync();

                var events = await _fileSystemService.ParseFiles(folders);

                TeslaEventMapLayer = new ObservableCollection<MapLayer>();
                var layer = new MapElementsLayer();
                foreach (var teslaEvent in events)
                {
                    MapIcon eventMapIcon = new MapIcon();
                    eventMapIcon.Location = new Geopoint(new BasicGeoposition() { Latitude = teslaEvent.EstimatedLatitude, Longitude = teslaEvent.EstimatedLongitude });
                    eventMapIcon.Tag = teslaEvent;
                    layer.MapElements.Add(eventMapIcon);
                }
                TeslaEventMapLayer.Add(layer);
            }

            IsBusy = false;
        }
    }
}
