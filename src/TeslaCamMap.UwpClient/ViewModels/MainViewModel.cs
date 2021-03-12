using System;
using System.Collections.ObjectModel;
using TeslaCamMap.Lib.Model;
using TeslaCamMap.UwpClient.Commands;
using TeslaCamMap.UwpClient.Model;
using TeslaCamMap.UwpClient.Services;
using Windows.Devices.Geolocation;
using Windows.Storage.Pickers;

namespace TeslaCamMap.UwpClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const int DefaultZoomLevel = 15;

        private UwpFileSystemService _fileSystemService;

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

        private int _processedEvents;
        public int ProcessedEvents
        {
            get { return _processedEvents; }
            set
            {
                _processedEvents = value;
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

        private TeslaEventMapElementViewModel _selectedTeslaEvent;
        public TeslaEventMapElementViewModel SelectedTeslaEvent
        {
            get { return _selectedTeslaEvent; }
            set
            {
                if (_selectedTeslaEvent != null)
                    _selectedTeslaEvent.IsSelected = false;

                if (value != null)
                {
                    _selectedTeslaEvent = value;
                    _selectedTeslaEvent.IsSelected = true;
                    MapCenter = _selectedTeslaEvent.Location;
                }

                OnPropertyChanged();

                if (MapZoom < DefaultZoomLevel)
                    MapZoom = DefaultZoomLevel;
            }
        }

        private ObservableCollection<TeslaEventMapElementViewModel> _teslaEvents;
        public ObservableCollection<TeslaEventMapElementViewModel> TeslaEvents
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
        public RelayCommand SelectEventCommand { get; set; }

        public MainViewModel()
        {
            _fileSystemService = new UwpFileSystemService();
            _fileSystemService.ProgressUpdated += _fileSystemService_ProgressUpdated;

            PickFolderCommand = new RelayCommand(PickFolderCommandExecute, CanPickFolderCommandExecute);
            ViewVideoCommand = new RelayCommand(ViewVideoCommandExecute, CanViewVideoCommandExecute);
            SelectEventCommand = new RelayCommand(SelectEventCommandExecute, CanSelectEventCommandExecute);
        }

        private void _fileSystemService_ProgressUpdated(object sender, ClientEventArgs.ProgressEventArgs e)
        {
            ProcessedEvents = e.ItemsCompleted;
        }

        private bool CanSelectEventCommandExecute(object arg)
        {
            return TeslaEvents != null && TeslaEvents.Count > 0;
        }

        private void SelectEventCommandExecute(object obj)
        {
            SelectedTeslaEvent = (TeslaEventMapElementViewModel)obj;
        }

        private bool CanViewVideoCommandExecute(object arg)
        {
            return SelectedTeslaEvent != null;
        }

        private void ViewVideoCommandExecute(object obj)
        {
            UwpTeslaEvent teslaEvent = ((TeslaEventMapElementViewModel)obj).Model;
            _fileSystemService.PopulateEventMetadata(teslaEvent);
            ViewFrame.Navigate(typeof(EventDetailsPage), teslaEvent);
        }

        private bool CanPickFolderCommandExecute(object arg)
        {
            return !IsBusy;
        }

        public async void OnLoaded()
        {
            if (String.IsNullOrEmpty(BingMapServiceToken))
            {
                BingMapServiceToken = await _fileSystemService.GetStringFromApplicationFile("bing_key");
                SelectedFolderLabelText = "No folder selected";
            }
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
                TeslaEvents = new ObservableCollection<TeslaEventMapElementViewModel>();
                events.ForEach(e => TeslaEvents.Add(new TeslaEventMapElementViewModel(e)));

                SelectedFolderLabelText = $"{result.Path} - {TeslaEvents.Count} events found.";
            }

            IsBusy = false;
        }
    }
}
