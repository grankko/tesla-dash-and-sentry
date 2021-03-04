using FFmpegInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TeslaCamMap.Lib.Model;
using TeslaCamMap.UwpClient.Model;
using TeslaCamMap.UwpClient.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TeslaCamMap.UwpClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EventDetailsPage : Page
    {
        private const int BufferSizeInBytes = 1048576;

        private FFmpegInteropMSS _leftFfmpegInterop;
        private FFmpegInteropMSS _frontFfmpegInterop;
        private FFmpegInteropMSS _rightFfmpegInterop;
        private FFmpegInteropMSS _backFfmpegInterop;

        private DispatcherTimer _timer;
        private List<MediaPlayerElement> _players = new List<MediaPlayerElement>();

        public EventDetailsPage()
        {
            this.InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += _timer_Tick;

            _players.Add(LeftPlayer);
            _players.Add(FrontPlayer);
            _players.Add(RightPlayer);
            _players.Add(BackPlayer);
        }

        private void _timer_Tick(object sender, object e)
        {
            VideoSlider.Value = LeftPlayer.MediaPlayer.PlaybackSession.Position.TotalSeconds;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var vm = new EventDetailsViewModel((UwpTeslaEvent)e.Parameter);
            this.DataContext = vm;

            vm.PlayVideo += Vm_PlayVideo;
            vm.PauseVideo += Vm_PauseVideo;
            vm.LoadClip += Vm_LoadClip;

            vm.OnNavigated();
        }

        private void Vm_LoadClip(object sender, LoadClipEventArgs e)
        {
            VideoSlider.Value = 0;
            foreach (var clip in e.Clip.Clips)
                LoadClip(clip);
        }

        private async void LoadClip(UwpClip clip)
        {
            FFmpegInteropConfig conf = new FFmpegInteropConfig();
            conf.StreamBufferSize = BufferSizeInBytes;

            using (var stream = await clip.ClipFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                switch (clip.Camera)
                {
                    case Camera.LeftRepeater:
                        _leftFfmpegInterop = await FFmpegInteropMSS.CreateFromStreamAsync(stream, conf);
                        LeftPlayer.Source = _leftFfmpegInterop.CreateMediaPlaybackItem();
                        VideoSlider.Minimum = 0;
                        VideoSlider.Maximum = _leftFfmpegInterop.Duration.TotalSeconds;
                        break;
                    case Camera.Front:
                        _frontFfmpegInterop = await FFmpegInteropMSS.CreateFromStreamAsync(stream, conf);
                        FrontPlayer.Source = _frontFfmpegInterop.CreateMediaPlaybackItem();
                        break;
                    case Camera.RightRepeater:
                        _rightFfmpegInterop = await FFmpegInteropMSS.CreateFromStreamAsync(stream, conf);
                        RightPlayer.Source = _rightFfmpegInterop.CreateMediaPlaybackItem();
                        break;
                    case Camera.Back:
                        _backFfmpegInterop = await FFmpegInteropMSS.CreateFromStreamAsync(stream, conf);
                        BackPlayer.Source = _backFfmpegInterop.CreateMediaPlaybackItem();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private void Vm_PauseVideo(object sender, EventArgs e)
        {
            _players.ForEach(p => p.MediaPlayer.Pause());

            _timer.Stop();
        }

        private void Vm_PlayVideo(object sender, EventArgs e)
        {
            _players.ForEach(p => p.MediaPlayer.Play());

            _timer.Start();
        }

        private void VideoSlider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _players.ForEach(p => p.MediaPlayer.Pause());

            _timer.Stop();
        }

        private void VideoSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var newValue = VideoSlider.Value;
            _players.ForEach(p => p.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(newValue));

            var vm = (EventDetailsViewModel)this.DataContext;
            if (vm.IsPlaying)
            {
                vm.PlayVideoCommand.Execute(null);
                _timer.Start();
            }
        }

        private void ClipsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClipsListView.ScrollIntoView(ClipsListView.SelectedItem);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((EventDetailsViewModel)DataContext).ViewFrame = this.Frame;
        }
    }
}
