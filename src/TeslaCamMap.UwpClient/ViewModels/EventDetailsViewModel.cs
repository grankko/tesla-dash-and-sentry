using System;
using System.Collections.ObjectModel;
using System.Linq;
using TeslaCamMap.Lib.Model;
using TeslaCamMap.UwpClient.ClientEventArgs;
using TeslaCamMap.UwpClient.Commands;
using TeslaCamMap.UwpClient.Model;

namespace TeslaCamMap.UwpClient.ViewModels
{
    public class EventDetailsViewModel : ViewModelBase
    {
        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                PlayVideoCommand.RaiseCanExecuteChanged();
                PauseVideoCommand.RaiseCanExecuteChanged();
                StepFrameCommand.RaiseCanExecuteChanged();
            }
        }

        // Events for stuff that needs to happen in the view (for now)
        public event EventHandler PlayVideo;
        public event EventHandler PauseVideo;
        public event EventHandler<StepFrameEventArgs> StepFrame;
        public event EventHandler<LoadSegmentEventArgs> LoadSegment;

        public RelayCommand PlayVideoCommand { get; set; }
        public RelayCommand PauseVideoCommand { get; set; }
        public RelayCommand NextSegmentCommand { get; set; }
        public RelayCommand PreviousSegmentCommand { get; set; }
        public RelayCommand NavigateToMapCommand { get; set; }
        public RelayCommand StepFrameCommand { get; set; }

        public ObservableCollection<EventSegmentViewModel> Segments { get; set; }

        private EventSegmentViewModel _currentSegment;

        public EventSegmentViewModel CurrentSegment
        {
            get { return _currentSegment; }
            set
            {
                // Pause video if a new clip is loaded
                if (_currentSegment != null)
                    PauseVideoCommand.Execute(null);

                _currentSegment = value;
                OnPropertyChanged();

                NextSegmentCommand.RaiseCanExecuteChanged();
                PreviousSegmentCommand.RaiseCanExecuteChanged();

                // Signals view to load videos into players
                if (LoadSegment != null)
                    LoadSegment.Invoke(this, new LoadSegmentEventArgs(_currentSegment));
            }
        }

        public EventDetailsViewModel(UwpTeslaEvent model)
        {

            NextSegmentCommand = new RelayCommand(NextSegmentCommandExecute, CanNextSegmentCommandExecute);
            PreviousSegmentCommand = new RelayCommand(PreviousSegmentCommandExecute, CanPreviousSegmentCommandExecute);
            NavigateToMapCommand = new RelayCommand(NavigateToMapCommandExecute, (o) => true);
            PlayVideoCommand = new RelayCommand(PlayVideoCommandExecute, CanPlayVideoCommandExecute);
            PauseVideoCommand = new RelayCommand(PauseVideoCommandExecute, CanPauseVideoCommandExecute);
            StepFrameCommand = new RelayCommand(StepFrameCommandExecute, CanStepFrameCommandExecute);

            Segments = new ObservableCollection<EventSegmentViewModel>();
            int index = 0;
            foreach (var segment in model.Segments)
            {
                var segmentViewModel = new EventSegmentViewModel(segment);
                segmentViewModel.SegmentIndex = index;
                Segments.Add(segmentViewModel);

                index++;
            }
        }

        private bool CanStepFrameCommandExecute(object arg)
        {
            return !IsPlaying;
        }

        private void StepFrameCommandExecute(object obj)
        {
            StepFrame?.Invoke(this, new StepFrameEventArgs((bool)obj));
        }

        public void OnNavigated()
        {
            CurrentSegment = Segments.First();
        }

        private bool CanPauseVideoCommandExecute(object arg)
        {
            return IsPlaying;
        }

        private void PauseVideoCommandExecute(object obj)
        {
            IsPlaying = false;
            PauseVideo?.Invoke(this, new EventArgs());
        }

        private bool CanPlayVideoCommandExecute(object arg)
        {
            return !IsPlaying;
        }

        private void PlayVideoCommandExecute(object obj)
        {
            IsPlaying = true;
            PlayVideo?.Invoke(this, new EventArgs());
        }

        private void NavigateToMapCommandExecute(object obj)
        {
            ViewFrame.Navigate(typeof(MainPage), obj);
        }

        private bool CanPreviousSegmentCommandExecute(object arg)
        {
            return (CurrentSegment != null && Segments.Min(c => c.SegmentIndex) != CurrentSegment.SegmentIndex);
        }

        private void PreviousSegmentCommandExecute(object obj)
        {
            CurrentSegment = Segments.Single(c => c.SegmentIndex == (CurrentSegment.SegmentIndex - 1));
        }

        private bool CanNextSegmentCommandExecute(object arg)
        {
            return (CurrentSegment != null && Segments.Max(c => c.SegmentIndex) != CurrentSegment.SegmentIndex);
        }

        private void NextSegmentCommandExecute(object obj)
        {
            CurrentSegment = Segments.Single(c => c.SegmentIndex == (CurrentSegment.SegmentIndex + 1));
        }
    }
}
