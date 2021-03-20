using System;
using System.Collections.ObjectModel;
using TeslaCamMap.UwpClient.Commands;
using TeslaCamMap.UwpClient.Model;
using TeslaCamMap.UwpClient.Services;
using Windows.Devices.Geolocation;

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

        private int _processedEvents;
        public int ProcessedEvents
        {
            get { return _processedEvents; }
            set
            {
                _processedEvents = value;
                OnPropertyChanged();
                OnPropertyChanged("ProcessedEventsOfTotal");
            }
        }

        private int _totalEvents;
        public int TotalEvents
        {
            get { return _totalEvents; }
            set
            {
                _totalEvents = value;
                OnPropertyChanged();
                OnPropertyChanged("ProcessedEventsOfTotal");
            }
        }

        public string ProcessedEventsOfTotal
        {
            get { return String.Format("{0}/{1}", _processedEvents, _totalEvents); }
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
            _fileSystemService = new FileSystemService();
            _fileSystemService.ProgressUpdated += _fileSystemService_ProgressUpdated;

            PickFolderCommand = new RelayCommand(PickFolderCommandExecute, CanPickFolderCommandExecute);
            ViewVideoCommand = new RelayCommand(ViewVideoCommandExecute, CanViewVideoCommandExecute);
            SelectEventCommand = new RelayCommand(SelectEventCommandExecute, CanSelectEventCommandExecute);
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

        private async void ViewVideoCommandExecute(object obj)
        {
            TeslaEvent teslaEvent = ((TeslaEventMapElementViewModel)obj).Model;
            ViewFrame.Navigate(typeof(EventDetailsPage), await _fileSystemService.PopulateEventMetadata(teslaEvent));
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
            ProcessedEvents = 0;
            
            var result = await _fileSystemService.OpenAndParseFolder();
            if (result?.Result != null)
            {
                TeslaEvents = new ObservableCollection<TeslaEventMapElementViewModel>();
                result.Result.ForEach(e => TeslaEvents.Add(new TeslaEventMapElementViewModel(e)));

                SelectedFolderLabelText = $"{result.ParsedPath} - {TeslaEvents.Count} events found.";
            }

            IsBusy = false;
        }

        private void _fileSystemService_ProgressUpdated(object sender, ClientEventArgs.ProgressEventArgs e)
        {
            IsBusy = true;
            ProcessedEvents = e.ItemsCompleted;
            TotalEvents = e.ItemsTotal;
        }
    }
}
