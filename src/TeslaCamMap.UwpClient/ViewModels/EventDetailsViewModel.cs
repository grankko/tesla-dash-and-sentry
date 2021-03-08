using System;
using System.Collections.ObjectModel;
using System.Linq;
using TeslaCamMap.Lib.Model;
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
        public event EventHandler<LoadClipEventArgs> LoadClip;

        public RelayCommand PlayVideoCommand { get; set; }
        public RelayCommand PauseVideoCommand { get; set; }
        public RelayCommand NextClipCommand { get; set; }
        public RelayCommand PreviousClipCommand { get; set; }
        public RelayCommand NavigateToMapCommand { get; set; }
        public RelayCommand StepFrameCommand { get; set; }

        public ObservableCollection<ClipViewModel> Clips { get; set; }

        private ClipViewModel _currentClip;

        public ClipViewModel CurrentClip
        {
            get { return _currentClip; }
            set
            {
                // Pause video if a new clip is loaded
                if (_currentClip != null)
                    PauseVideoCommand.Execute(null);

                _currentClip = value;
                OnPropertyChanged();

                NextClipCommand.RaiseCanExecuteChanged();
                PreviousClipCommand.RaiseCanExecuteChanged();

                // Signals view to load videos into players
                if (LoadClip != null)
                    LoadClip.Invoke(this, new LoadClipEventArgs(_currentClip));
            }
        }

        public EventDetailsViewModel(UwpTeslaEvent model)
        {

            NextClipCommand = new RelayCommand(NextClipCommandExecute, CanNextClipCommandExecute);
            PreviousClipCommand = new RelayCommand(PreviousClipCommandExecute, CanPreviousClipCommandExecute);
            NavigateToMapCommand = new RelayCommand(NavigateToMapCommandExecute, CanNavigateToMapCommandExecute);
            PlayVideoCommand = new RelayCommand(PlayVideoCommandExecute, CanPlayVideoCommandExecute);
            PauseVideoCommand = new RelayCommand(PauseVideoCommandExecute, CanPauseVideoCommandExecute);
            StepFrameCommand = new RelayCommand(StepFrameCommandExecute, CanStepFrameCommandExecute);

            Clips = new ObservableCollection<ClipViewModel>();

            // Find all unique video segments and populate ClipViewModels.
            // todo: this should have been modeled better
            var leftRepeaterClips = model.Clips.Cast<UwpClip>().Where(c => c.Camera == Camera.LeftRepeater).OrderBy(c => c.FileName).ToList();
            int index = 0;
            foreach (var clip in leftRepeaterClips)
            {
                var clipViewModel = new ClipViewModel();
                clipViewModel.CommonFileNameSegment = clip.FileName.Substring(0, 19);
                clipViewModel.ClipIndex = index;
                clipViewModel.TimeStamp = clip.TimeStampFromFileName;

                var allAngleClips = model.Clips.Cast<UwpClip>().Where(c => c.FileName.Contains(clipViewModel.CommonFileNameSegment));
                clipViewModel.Clips = allAngleClips.ToList();

                Clips.Add(clipViewModel);
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
            CurrentClip = Clips.First();
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
            return (!IsPlaying);
        }

        private void PlayVideoCommandExecute(object obj)
        {
            IsPlaying = true;
            PlayVideo?.Invoke(this, new EventArgs());
        }

        private bool CanNavigateToMapCommandExecute(object arg)
        {
            return true;
        }

        private void NavigateToMapCommandExecute(object obj)
        {
            ViewFrame.Navigate(typeof(MainPage), obj);
        }

        private bool CanPreviousClipCommandExecute(object arg)
        {
            return (CurrentClip != null && Clips.Min(c => c.ClipIndex) != CurrentClip.ClipIndex);
        }

        private void PreviousClipCommandExecute(object obj)
        {
            CurrentClip = Clips.Single(c => c.ClipIndex == (CurrentClip.ClipIndex - 1));
        }

        private bool CanNextClipCommandExecute(object arg)
        {
            return (CurrentClip != null && Clips.Max(c => c.ClipIndex) != CurrentClip.ClipIndex);
        }

        private void NextClipCommandExecute(object obj)
        {
            CurrentClip = Clips.Single(c => c.ClipIndex == (CurrentClip.ClipIndex + 1));
        }
    }
}
