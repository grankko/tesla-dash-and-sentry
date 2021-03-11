using TeslaCamMap.Lib.Model;

namespace TeslaCamMap.UwpClient.ViewModels
{
    public class VideoPlayerViewModel : ViewModelBase
    {
        private bool _isInFullscreen;

        public bool IsInFullscreen
        {
            get => _isInFullscreen;
            set {
                _isInFullscreen = value;
                OnPropertyChanged();
            }
        }

        public Camera CameraAngle { get; private set; }

        public VideoPlayerViewModel(Camera camera)
        {
            CameraAngle = camera;
        }
    }
}
