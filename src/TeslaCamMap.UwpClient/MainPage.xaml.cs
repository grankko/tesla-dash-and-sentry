using TeslaCamMap.UwpClient.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TeslaCamMap.UwpClient
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((MainViewModel)DataContext).ViewFrame = this.Frame;
            ((MainViewModel)DataContext).OnLoaded();
        }

        private void EventsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EventsListView.ScrollIntoView(EventsListView.SelectedItem);
        }

        private void EventsListView_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            ((MainViewModel)DataContext).ViewVideoCommand.Execute(EventsListView.SelectedItem);
        }
    }
}
