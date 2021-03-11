using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace TeslaCamMap.UwpClient.Controls
{
    public sealed partial class VideoPlayerControl : UserControl
    {
        public MediaPlayerElement VideoPlayerElement { get => PlayerElement; }
        public event EventHandler ToggleFullscreen;

        public VideoPlayerControl()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((VideoPlayerViewModel)DataContext).IsInFullscreen = !((VideoPlayerViewModel)DataContext).IsInFullscreen;
            ToggleFullscreen?.Invoke(this, new EventArgs());
        }
    }
}
