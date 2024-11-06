using Google.Maps.Utils;
using Maui.GoogleMaps.iOS.Factories;
using Maui.GoogleMaps.Views;
using UIKit;

namespace Maui.GoogleMaps.Platforms.iOS.Renderers
{
    public class ClusterMarkerIconGenerator : GMUDefaultClusterIconGenerator
    {
        private readonly IImageFactory _imageFactory;
        private readonly IElementHandler _handler;
        private readonly ContentView _noClusterView;
        private readonly ContentView _clusterView;

        public ClusterMarkerIconGenerator(IImageFactory imageFactory, ContentView noClusterView, ContentView clusterView, IElementHandler handler) : base()
        {
            _imageFactory = imageFactory;
            _noClusterView = noClusterView;
            _clusterView = clusterView;
            _handler = handler;
        }

        public override UIImage IconForSize(nuint size)
        {

            var number = (int)size;
            if (number > 1)
            {
                if (_clusterView == null)
                {
                    return base.IconForSize(size);
                }
                try
                {
                    var iconSize = GetSizeByCount(number);
                    _clusterView.HeightRequest = iconSize;
                    _clusterView.WidthRequest = iconSize;
                    ((IClusterView)_clusterView).ChangeClusterText(GetLabelByCount(number));
                    var icon = BitmapDescriptorFactory.FromView(() => _clusterView);
                    var nativeDescriptor = _imageFactory.ToUIImage(icon, _handler.MauiContext);
                    return nativeDescriptor;
                }
                catch
                {
                    return base.IconForSize(size);
                }
            }
            else
            {
                if (_noClusterView == null)
                {
                    return base.IconForSize(size);
                }
                ((IClusterView)_noClusterView).ChangeSize(64);
                var icon = BitmapDescriptorFactory.FromView(() => _noClusterView);
                try
                {
                    var nativeDescriptor = _imageFactory.ToUIImage(icon, _handler.MauiContext);
                    return nativeDescriptor;
                }
                catch
                {
                    return base.IconForSize(size);
                }
            }
        }

        private static int GetSizeByCount(int count)
        {
            if (count < 100)
            {
                return 40;
            }
            else if (count < 1000)
            {
                return 48;
            }
            else
            {
                return 56;
            }
        }

        private static string GetLabelByCount(int count)
        {
            if (count < 11)
            {
                return $"{count}";
            }
            else if (count > 10 && count < 21)
            {
                return "10+";
            }
            else if (count > 20 && count < 51)
            {
                return "20+";
            }
            else if (count > 50 && count < 101)
            {
                return "50+";
            }
            else if (count > 100 && count < 501)
            {
                return "100+";
            }
            else if (count > 500 && count < 1001)
            {
                return "500+";
            }
            else
            {
                return "1000+";
            }
        }
    }
}

