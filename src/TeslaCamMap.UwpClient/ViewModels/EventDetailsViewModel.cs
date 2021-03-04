using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeslaCamMap.Lib.Model;
using TeslaCamMap.UwpClient.Commands;
using TeslaCamMap.UwpClient.Model;

namespace TeslaCamMap.UwpClient.ViewModels
{
    public class EventDetailsViewModel : ViewModelBase
    {
        public bool IsPlaying { get; set; }

        public event EventHandler PlayVideo;
        public event EventHandler PauseVideo;
        public event EventHandler<LoadClipEventArgs> LoadClip;

        public RelayCommand PlayVideoCommand { get; set; }
        public RelayCommand PauseVideoCommand { get; set; }

        public RelayCommand NextClipCommand { get; set; }
        public RelayCommand PreviousClipCommand { get; set; }
        public RelayCommand NavigateToMapCommand { get; set; }

        public ObservableCollection<ClipViewModel> Clips { get; set; }

        private ClipViewModel _currentClip;
        public ClipViewModel CurrentClip
        {
            get { return _currentClip; }
            set
            {
                if (_currentClip != null)
                    PauseVideoCommand.Execute(null);

                _currentClip = value;
                OnPropertyChanged();
                NextClipCommand.RaiseCanExecuteChanged();
                PreviousClipCommand.RaiseCanExecuteChanged();

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

            Clips = new ObservableCollection<ClipViewModel>();

            var leftRepeaterClips = model.Clips.Cast<UwpClip>().Where(c => c.Camera == Camera.LeftRepeater).OrderBy(c => c.FileName).ToList();
            int index = 0;
            foreach (var clip in leftRepeaterClips)
            {
                var clipViewModel = new ClipViewModel();
                clipViewModel.FileName = clip.FileName.Substring(0, 19);
                clipViewModel.ClipIndex = index;

                var allAngleClips = model.Clips.Cast<UwpClip>().Where(c => c.FileName.Contains(clipViewModel.FileName));
                clipViewModel.Clips = allAngleClips.ToList();

                Clips.Add(clipViewModel);
                index++;
            }
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
            PlayVideoCommand.RaiseCanExecuteChanged();
            PauseVideoCommand.RaiseCanExecuteChanged();

            if (PauseVideo != null)
                PauseVideo.Invoke(this, new EventArgs());
        }

        private bool CanPlayVideoCommandExecute(object arg)
        {
            return (!IsPlaying);
        }

        private void PlayVideoCommandExecute(object obj)
        {
            IsPlaying = true;
            PlayVideoCommand.RaiseCanExecuteChanged();
            PauseVideoCommand.RaiseCanExecuteChanged();

            if (PlayVideo != null)
                PlayVideo.Invoke(this, new EventArgs());
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
