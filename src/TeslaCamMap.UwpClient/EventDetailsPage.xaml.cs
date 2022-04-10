using FFmpegInterop;
using System;
using System.Threading.Tasks;
using TeslaCamMap.UwpClient.ClientEventArgs;
using TeslaCamMap.UwpClient.Controls;
using TeslaCamMap.UwpClient.Model;
using TeslaCamMap.UwpClient.ViewModels;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace TeslaCamMap.UwpClient
{
    public sealed partial class EventDetailsPage : Page
    {
        private const int BufferSizeInBytes = 1048576;
        private const int SliderTimerUpdateInterval = 500;

        // One instance per video seems to work best.
        // A reference to the FFMpegInteropMSS needs to be maintained for the players to be stable.
        private FFmpegInteropMSS _leftFfmpegInterop;
        private FFmpegInteropMSS _frontFfmpegInterop;
        private FFmpegInteropMSS _rightFfmpegInterop;
        private FFmpegInteropMSS _backFfmpegInterop;

        private MediaTimelineController _mediaTimelineController = new MediaTimelineController();
        private int _currentEstimatedFrameDuration;

        private DispatcherTimer _timer; // Used to update the video slider when video is playing

        public EventDetailsPage()
        {
            this.InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(SliderTimerUpdateInterval);
            _timer.Tick += _timer_Tick;
        }

        private void _timer_Tick(object sender, object e)
        {
            VideoSlider.Value = _mediaTimelineController.Position.TotalSeconds;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var vm = new EventDetailsViewModel((TeslaEvent)e.Parameter);
            this.DataContext = vm;

            vm.PlayVideo += Vm_PlayVideo;
            vm.PauseVideo += Vm_PauseVideo;
            vm.LoadSegment += Vm_LoadSegment;
            vm.StepFrame += Vm_StepFrame;
            vm.ChangePlaybackSpeed += Vm_ChangePlaybackSpeed;

            vm.OnNavigated();
        }

        private void Vm_ChangePlaybackSpeed(object sender, PlaybackSpeedChangedEventArgs e)
        {
            _mediaTimelineController.ClockRate = e.NewPlaybackSpeed;
        }

        private void Vm_StepFrame(object sender, StepFrameEventArgs e)
        {
            if (e.StepForward)
                _mediaTimelineController.Position += TimeSpan.FromMilliseconds(_currentEstimatedFrameDuration);
            else
                _mediaTimelineController.Position -= TimeSpan.FromMilliseconds(_currentEstimatedFrameDuration);

            VideoSlider.Value = LeftPlayer.VideoPlayerElement.MediaPlayer.PlaybackSession.Position.TotalSeconds;
        }

        private void Vm_LoadSegment(object sender, LoadSegmentEventArgs e)
        {
            _currentEstimatedFrameDuration = (int)e.Segment.Model.MaxClipFrameDuration;
            VideoSlider.Value = 0;
            foreach (var clip in e.Segment.Model.Clips)
                LoadClip(clip);

            VideoSlider.Minimum = 0;
            VideoSlider.Maximum = e.Segment.Model.MaxClipDuration.Value.TotalSeconds;
        }

        private async void LoadClip(Clip clip)
        {
            switch (clip.Camera)
            {
                case Camera.LeftRepeater:
                    _leftFfmpegInterop = await CreateMediaSourceAndPlayer(LeftPlayer.VideoPlayerElement, _leftFfmpegInterop, clip);
                    break;
                case Camera.Front:
                    _frontFfmpegInterop = await CreateMediaSourceAndPlayer(FrontPlayer.VideoPlayerElement, _frontFfmpegInterop, clip);
                    break;
                case Camera.RightRepeater:
                    _rightFfmpegInterop = await CreateMediaSourceAndPlayer(RightPlayer.VideoPlayerElement, _rightFfmpegInterop, clip);
                    break;
                case Camera.Back:
                    _backFfmpegInterop = await CreateMediaSourceAndPlayer(BackPlayer.VideoPlayerElement, _backFfmpegInterop, clip);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            _mediaTimelineController.Position = TimeSpan.Zero;
        }

        private async Task<FFmpegInteropMSS> CreateMediaSourceAndPlayer(MediaPlayerElement playerElement, FFmpegInteropMSS ffmpegMss, Clip clip)
        {
            if (ffmpegMss != null)
                ffmpegMss.Dispose();

            FFmpegInteropConfig conf = new FFmpegInteropConfig();
            conf.StreamBufferSize = BufferSizeInBytes;

            MediaPlayer player = playerElement.MediaPlayer;
            if (player == null)
                player = CreatePlayer();

            using (var stream = await clip.ClipFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                ffmpegMss = await FFmpegInteropMSS.CreateFromStreamAsync(stream, conf);
            }

            player.Source = ffmpegMss.CreateMediaPlaybackItem();
            playerElement.SetMediaPlayer(player);

            return ffmpegMss;
        }

        private MediaPlayer CreatePlayer()
        {
            var player = new MediaPlayer();
            player.CommandManager.IsEnabled = false;
            player.TimelineController = _mediaTimelineController; // Synchronize all players with the same MediaTimelineController
            return player;
        }

        private void Vm_PauseVideo(object sender, EventArgs e)
        {
            // Pause video when event raised in ViewModel
            _timer.Stop();
            _mediaTimelineController.Pause();
        }

        private void Vm_PlayVideo(object sender, EventArgs e)
        {
            // Play video when event raised in ViewModel
            _mediaTimelineController.Resume();
            _timer.Start();
        }

        private void VideoSlider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            // Pause video when user starts interacting with the slider thumb
            _timer.Stop();
            _mediaTimelineController.Pause();
        }

        private void VideoSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            UpdateVideoPositionOnSliderInteraction();
        }

        private void VideoSlider_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UpdateVideoPositionOnSliderInteraction();
        }

        private void UpdateVideoPositionOnSliderInteraction()
        {
            // Set playback position for each video when user have moved the slider thumb
            var newValue = VideoSlider.Value;
            _mediaTimelineController.Position = TimeSpan.FromSeconds(newValue);

            var vm = (EventDetailsViewModel)this.DataContext;
            if (vm.IsPlaying)
            {
                // Resume playing if the videos was playing before interactions started
                vm.PlayVideoCommand.Execute(null);
                _timer.Start();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((EventDetailsViewModel)DataContext).ViewFrame = this.Frame;
        }

        private void SegmentsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SegmentsListView.ScrollIntoView(SegmentsListView.SelectedItem);
        }

        private void SegmentsListView_Loaded(object sender, RoutedEventArgs e)
        {
            SegmentsListView.ScrollIntoView(SegmentsListView.SelectedItem);
        }

        private void Player_ToggleFullscreen(object sender, EventArgs e)
        {
            VideoPlayerControl player = (VideoPlayerControl)sender;

            if (OneCameraLayoutGrid.Children.Contains(player))
            {
                OneCameraLayoutGrid.Children.Remove(player);
                OneCameraLayoutGrid.Visibility = Visibility.Collapsed;

                MultiCameraLayoutGrid.Visibility = Visibility.Visible;
                MultiCameraLayoutGrid.Children.Add(player);
            }
            else
            {
                MultiCameraLayoutGrid.Visibility = Visibility.Collapsed;
                MultiCameraLayoutGrid.Children.Remove(player);

                OneCameraLayoutGrid.Children.Add(player);
                OneCameraLayoutGrid.Visibility = Visibility.Visible;
            }
        }

        private void VideoSlider_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((EventDetailsViewModel)DataContext).SliderWidth = e.NewSize.Width;
        }

        private void MinimizeControlsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ControlsGrid.Visibility == Visibility.Collapsed)
            {
                ControlsGrid.Visibility = Visibility.Visible;
                ControlsRow.Height = new GridLength(200);
                MinimizeControlsIcon.Glyph = "\xE972";
            } else
            {
                ControlsGrid.Visibility = Visibility.Collapsed;
                ControlsRow.Height = new GridLength(0);
                MinimizeControlsIcon.Glyph = "\xE971";
            }
        }
    }
}
