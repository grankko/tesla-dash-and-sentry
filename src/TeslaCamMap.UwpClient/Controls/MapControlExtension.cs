using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace TeslaCamMap.UwpClient.Controls
{
    public class MapControlExtension : DependencyObject
    {

        #region Dependency properties

        public static readonly DependencyProperty ZIndexProperty = DependencyProperty.RegisterAttached("ZIndex", typeof(int), typeof(MapControlExtension), new PropertyMetadata(0, OnZIndexChanged));

        #endregion


        #region Methods

        public static int GetZIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(ZIndexProperty);
        }

        private static void OnZIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ContentControl us = d as ContentControl;

            if (us != null)
            {
                // returns null if it is not loaded yet
                // returns 'DependencyObject' of type 'MapOverlayPresenter'
                var parent = VisualTreeHelper.GetParent(us);

                if (parent != null)
                {
                    parent.SetValue(Canvas.ZIndexProperty, e.NewValue);
                }
            }
        }

        public static void SetZIndex(DependencyObject obj, int value)
        {
            obj.SetValue(ZIndexProperty, value);
        }

        #endregion

    }
}


