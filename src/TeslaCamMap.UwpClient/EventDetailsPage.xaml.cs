using FFmpegInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        private FFmpegInteropMSS _leftFfmpegInterop;
        private FFmpegInteropMSS _frontFfmpegInterop;
        private FFmpegInteropMSS _rightFfmpegInterop;
        private FFmpegInteropMSS _backFfmpegInterop;

        private DispatcherTimer _timer;

        public EventDetailsPage()
        {
            this.InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += _timer_Tick;
        }

        private void _timer_Tick(object sender, object e)
        {
            VideoSlider.Value = LeftPlayer.MediaPlayer.PlaybackSession.Position.TotalSeconds;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.DataContext = new EventDetailsViewModel((UwpTeslaEvent)e.Parameter);

            var model = (UwpTeslaEvent)e.Parameter;

            var leftFile = model.Clips.Cast<UwpClip>().Where(c => c.Camera == Lib.Model.Camera.LeftRepeater).OrderBy(c => c.FileName).First();
            var rightFile = model.Clips.Cast<UwpClip>().Where(c => c.Camera == Lib.Model.Camera.RightRepeater).OrderBy(c => c.FileName).First();
            var frontFile = model.Clips.Cast<UwpClip>().Where(c => c.Camera == Lib.Model.Camera.Front).OrderBy(c => c.FileName).First();
            var backFile = model.Clips.Cast<UwpClip>().Where(c => c.Camera == Lib.Model.Camera.Back).OrderBy(c => c.FileName).First();
            
            FFmpegInteropConfig conf = new FFmpegInteropConfig();
            conf.DefaultBufferTime = TimeSpan.FromSeconds(60);

            using (var stream = await leftFile.ClipFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                _leftFfmpegInterop = await FFmpegInteropMSS.CreateFromStreamAsync(stream, conf);
                LeftPlayer.Source = _leftFfmpegInterop.CreateMediaPlaybackItem();

                VideoSlider.Minimum = 0;
                VideoSlider.Maximum = _leftFfmpegInterop.Duration.TotalSeconds;
            }

            using (var stream = await rightFile.ClipFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                _rightFfmpegInterop = await FFmpegInteropMSS.CreateFromStreamAsync(stream, conf);
                RightPlayer.Source = _rightFfmpegInterop.CreateMediaPlaybackItem();
            }

            using (var stream = await frontFile.ClipFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                _frontFfmpegInterop = await FFmpegInteropMSS.CreateFromStreamAsync(stream, conf);
                FrontPlayer.Source = _frontFfmpegInterop.CreateMediaPlaybackItem();
            }

            using (var stream = await backFile.ClipFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                _backFfmpegInterop = await FFmpegInteropMSS.CreateFromStreamAsync(stream, conf);
                BackPlayer.Source = _backFfmpegInterop.CreateMediaPlaybackItem();
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            LeftPlayer.MediaPlayer.Play();
            FrontPlayer.MediaPlayer.Play();
            RightPlayer.MediaPlayer.Play();
            BackPlayer.MediaPlayer.Play();

            _timer.Start();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            LeftPlayer.MediaPlayer.Pause();
            FrontPlayer.MediaPlayer.Pause();
            RightPlayer.MediaPlayer.Pause();
            BackPlayer.MediaPlayer.Pause();

            _timer.Stop();
        }

        private void BackToMapButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void VideoSlider_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            LeftPlayer.MediaPlayer.Pause();
            FrontPlayer.MediaPlayer.Pause();
            RightPlayer.MediaPlayer.Pause();
            BackPlayer.MediaPlayer.Pause();

            _timer.Stop();
        }

        private void VideoSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var newValue = VideoSlider.Value;
            LeftPlayer.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(newValue);
            FrontPlayer.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(newValue);
            RightPlayer.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(newValue);
            BackPlayer.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(newValue);

            LeftPlayer.MediaPlayer.Play();
            FrontPlayer.MediaPlayer.Play();
            RightPlayer.MediaPlayer.Play();
            BackPlayer.MediaPlayer.Play();

            _timer.Start();
        }

        private void ClipsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClipsListView.ScrollIntoView(ClipsListView.SelectedItem);
        }
    }
}
