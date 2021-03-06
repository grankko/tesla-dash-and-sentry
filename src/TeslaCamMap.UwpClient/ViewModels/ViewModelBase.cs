using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Controls;

namespace TeslaCamMap.UwpClient.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public Frame ViewFrame { get; set; } // todo: introduce navigation service

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
