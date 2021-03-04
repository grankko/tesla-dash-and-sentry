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
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TeslaCamMap.UwpClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            //TeslaEventMap.MapServiceToken = "blZUM4oxWRB63DRW5wVT~QuicHoTczOcLusxVvXtoUA~AmHXiEOyNXCbK5rM1dQbXsYq4tu1rn3lBsnvhB7eyBRTTZiz9O3OsgjcU8ZOE_0I";
        }

        //todo: Center map zoom around 
        //await ActivityMap.TrySetViewBoundsAsync(GeoboundingBox.TryCompute(geopositions), null, MapAnimationKind.None);

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(EventDetailsPage));
        }

        private void MapControl_MapElementClick(MapControl sender, MapElementClickEventArgs args)
        {
            MapElement clickedItem = args.MapElements.First();
            ((MainViewModel)DataContext).OnMapElementClicked(clickedItem);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((MainViewModel)DataContext).ViewFrame = this.Frame;
            ((MainViewModel)DataContext).OnLoaded();
        }
    }
}
