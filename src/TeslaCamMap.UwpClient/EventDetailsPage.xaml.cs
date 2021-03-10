using FFmpegInterop;
using System;
using System.Collections.Generic;
using TeslaCamMap.Lib.Model;
using TeslaCamMap.UwpClient.ClientEventArgs;
using TeslaCamMap.UwpClient.Model;
using TeslaCamMap.UwpClient.ViewModels;
using Windows.Media;
using Windows.Media.Playback;
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
            // Update slider to represent the current position in the video
            // todo: Bind from ViewModel instead?
            VideoSlider.Value = LeftPlayer.MediaPlayer.PlaybackSession.Position.TotalSeconds;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var vm = new EventDetailsViewModel((UwpTeslaEvent)e.Parameter);
            this.DataContext = vm;

            vm.PlayVideo += Vm_PlayVideo;
            vm.PauseVideo += Vm_PauseVideo;
            vm.LoadSegment += Vm_LoadSegment;
            vm.StepFrame += Vm_StepFrame;

            vm.OnNavigated();
        }

        private void Vm_StepFrame(object sender, StepFrameEventArgs e)
        {
            if (e.StepForward)
                _mediaTimelineController.Position += TimeSpan.FromMilliseconds(_currentEstimatedFrameDuration);
            else
                _mediaTimelineController.Position -= TimeSpan.FromMilliseconds(_currentEstimatedFrameDuration);

            VideoSlider.Value = LeftPlayer.MediaPlayer.PlaybackSession.Position.TotalSeconds;
        }

        private void Vm_LoadSegment(object sender, LoadSegmentEventArgs e)
        {
            _currentEstimatedFrameDuration = (int)e.Segment.Model.MaxClipFrameDuration;
            VideoSlider.Value = 0;
            foreach (var clip in e.Segment.Model.Clips)
                LoadClip((UwpClip)clip);

            VideoSlider.Minimum = 0;
            VideoSlider.Maximum = e.Segment.Model.MaxClipDuration.TotalSeconds;
        }

        private async void LoadClip(UwpClip clip)
        {
            FFmpegInteropConfig conf = new FFmpegInteropConfig();
            conf.StreamBufferSize = BufferSizeInBytes;

            var player = new MediaPlayer();
            player.CommandManager.IsEnabled = false;
            player.TimelineController = _mediaTimelineController; // Synchronize all players with the same MediaTimelineController

            using (var stream = await clip.ClipFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                switch (clip.Camera)
                {
                    case Camera.LeftRepeater:
                        _leftFfmpegInterop = await FFmpegInteropMSS.CreateFromStreamAsync(stream, conf);
                        player.Source = _leftFfmpegInterop.CreateMediaPlaybackItem();
                        LeftPlayer.SetMediaPlayer(player);
                        break;
                    case Camera.Front:
                        _frontFfmpegInterop = await FFmpegInteropMSS.CreateFromStreamAsync(stream, conf);
                        player.Source = _frontFfmpegInterop.CreateMediaPlaybackItem();
                        FrontPlayer.SetMediaPlayer(player);
                        break;
                    case Camera.RightRepeater:
                        _rightFfmpegInterop = await FFmpegInteropMSS.CreateFromStreamAsync(stream, conf);
                        player.Source = _rightFfmpegInterop.CreateMediaPlaybackItem();
                        RightPlayer.SetMediaPlayer(player);
                        break;
                    case Camera.Back:
                        _backFfmpegInterop = await FFmpegInteropMSS.CreateFromStreamAsync(stream, conf);
                        player.Source = _backFfmpegInterop.CreateMediaPlaybackItem();
                        BackPlayer.SetMediaPlayer(player);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            _mediaTimelineController.Position = TimeSpan.Zero;
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
    }
}
