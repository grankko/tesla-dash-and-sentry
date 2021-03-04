using System;
using System.Collections.Generic;
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
        private UwpTeslaEvent _model;

        public RelayCommand NextClipCommand { get; set; }
        public RelayCommand PreviousClipCommand { get; set; }
        
        public ObservableCollection<ClipViewModel> Clips { get; set; }

        private ClipViewModel _currentClip;
        public ClipViewModel CurrentClip
        {
            get { return _currentClip; }
            set
            {
                _currentClip = value;
                OnPropertyChanged();
                NextClipCommand.RaiseCanExecuteChanged();
                PreviousClipCommand.RaiseCanExecuteChanged();
            }
        }

        public EventDetailsViewModel(UwpTeslaEvent model)
        {
            _model = model;

            NextClipCommand = new RelayCommand(NextClipCommandExecute, CanNextClipCommandExecute);
            PreviousClipCommand = new RelayCommand(PreviousClipCommandExecute, CanPreviousClipCommandExecute);

            Clips = new ObservableCollection<ClipViewModel>();

            var leftRepeaterClips = model.Clips.Where(c => c.Camera == Camera.LeftRepeater).OrderBy(c => c.FileName).ToList();
            int index = 0;
            foreach (var clip in leftRepeaterClips)
            {
                var clipViewModel = new ClipViewModel();
                clipViewModel.FileName = clip.FileName.Substring(0, 19);
                clipViewModel.ClipIndex = index;
                Clips.Add(clipViewModel);
                index++;
            }

            CurrentClip = Clips.First();
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

    public class ClipViewModel : ViewModelBase
    {
        public string FileName { get; set; }
        public int ClipIndex { get; set; }
    }
}
