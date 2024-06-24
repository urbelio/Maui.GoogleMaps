using Android.Content;
using Android.Gms.Maps;
using Com.Google.Maps.Android.Clustering;
using Com.Google.Maps.Android.Clustering.View;
using Android.Gms.Maps.Model;
using Android.Runtime;
using Com.Google.Maps.Android.Clustering.Algo;
using Maui.GoogleMaps.Android.Factories;
using Maui.GoogleMaps.Handlers;
using System.Collections;
using Maui.GoogleMaps.Logics.Android;
using Maui.GoogleMaps.Views;

namespace Maui.GoogleMaps.Platforms.Android.Renderers
{
    public class ClusterMarkerRenderer : DefaultClusterRenderer
    {
        private readonly IBitmapDescriptorFactory _bitmapDescriptorFactory;
        private readonly IElementHandler _handler;
        private readonly ContentView _noClusterView;
        private readonly ContentView _clusterView;
        private readonly ContentView _labelizedView;
        private double _previousZoom = 0;

        public ClusterMarkerRenderer(ContentView labelizedView, ContentView noClusterView, ContentView clusterView, Context context, IElementHandler handler, IBitmapDescriptorFactory bitmapDescriptorFactory, GoogleMap map, ClusterManager clusterManager) : base(context, map, clusterManager)
        {
            _bitmapDescriptorFactory = bitmapDescriptorFactory;
            _handler = handler;
            _noClusterView = noClusterView;
            _clusterView = clusterView;
            _labelizedView = labelizedView;
        }

        protected ClusterMarkerRenderer(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override bool ShouldRender(ICollection oldClusters, ICollection newClusters)
        {
            if (((MapHandler)_handler).VirtualView.CameraPosition.Zoom > 16)
            {
                _previousZoom = ((MapHandler)_handler).VirtualView.CameraPosition.Zoom;
                return true;
            }
            if (_previousZoom < 16)
            {
                _previousZoom = ((MapHandler)_handler).VirtualView.CameraPosition.Zoom;
                return true;
            }
            _previousZoom = ((MapHandler)_handler).VirtualView.CameraPosition.Zoom;
            return base.ShouldRender(oldClusters, newClusters);
        }

        protected override void OnBeforeClusterItemRendered(Java.Lang.Object item, MarkerOptions markerOptions)
        {
            if (((MapHandler)_handler).VirtualView.CameraPosition.Zoom > 17)
            {
                var clusterPin = (GoogleClusterPin)item;
                if (_labelizedView != null)
                {
                    var labelizedViewCopy = ((IClusterView)_labelizedView).Instance();
                    labelizedViewCopy.ChangeClusterText(clusterPin.Title);
                    var viewBMP = BitmapDescriptorFactory.FromView(() => (ContentView)labelizedViewCopy);
                    var nativeMarker = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                    markerOptions.SetIcon(nativeMarker);
                    markerOptions.SetTitle(clusterPin.Title);
                    markerOptions.SetSnippet(clusterPin.Snippet);
                }
            }
            else
            {
                if (_noClusterView == null)
                {
                    base.OnBeforeClusterItemRendered(item, markerOptions);
                }
                else
                {
                    var viewBMP = BitmapDescriptorFactory.FromView(() => _noClusterView.Content);
                    var nativeDescriptor = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                    markerOptions.SetIcon(nativeDescriptor);
                    var clusterPin = (GoogleClusterPin)item;
                    markerOptions.SetTitle(clusterPin.Title);
                    markerOptions.SetSnippet(clusterPin.Snippet);
                }
            }
        }

        protected override void OnClusterItemUpdated(Java.Lang.Object item, Marker marker)
        {
            var clusterPin = (GoogleClusterPin)item;
            if (((MapHandler)_handler).VirtualView.CameraPosition.Zoom > 17)
            {
                if (_labelizedView != null)
                {
                    var labelizedViewCopy = ((IClusterView)_labelizedView).Instance();
                    labelizedViewCopy.ChangeClusterText(clusterPin.Title);
                    var viewBMP = BitmapDescriptorFactory.FromView(() => (ContentView)labelizedViewCopy);
                    var nativeMarker = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                    marker.SetIcon(nativeMarker);
                    marker.Title = clusterPin.Title;
                    marker.Snippet = clusterPin.Snippet;
                    return;
                }
            }
            if (_noClusterView == null)
            {
                base.OnClusterItemUpdated(item, marker);
            }
            else
            {
                var viewBMP = BitmapDescriptorFactory.FromView(() => _noClusterView.Content);
                var nativeDescriptor = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                marker.SetIcon(nativeDescriptor);
                marker.Title = clusterPin.Title;
                marker.Snippet = clusterPin.Snippet;
            }
        }

        protected override void OnBeforeClusterRendered(ICluster cluster, MarkerOptions markerOptions)
        {
            if (_clusterView == null)
            {
                base.OnBeforeClusterRendered(cluster, markerOptions);
            }
            else
            {
                try
                {
                    var clusterPin = (StaticCluster)cluster;
                    var nativeDescriptor = GetNativeDescriptor(clusterPin);
                    markerOptions.Anchor(0.5f, 0.5f);
                    markerOptions.SetIcon(nativeDescriptor);
                }
                catch
                {
                    var viewBMP = BitmapDescriptorFactory.FromView(() => _noClusterView.Content);
                    var nativeDescriptor = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                    markerOptions.SetIcon(nativeDescriptor);
                }
            }
        }

        protected override void OnClusterUpdated(ICluster cluster, Marker marker)
        {
            if (_clusterView == null)
            {
                base.OnClusterUpdated(cluster, marker);
            }
            else
            {
                try
                {
                    var clusterPin = (StaticCluster)cluster;
                    var nativeDescriptor = GetNativeDescriptor(clusterPin);
                    marker.SetIcon(nativeDescriptor);
                }
                catch
                {
                    var viewBMP = BitmapDescriptorFactory.FromView(() => _noClusterView.Content);
                    var nativeDescriptor = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                    marker.SetIcon(nativeDescriptor);
                }
            }
        }

        protected override bool ShouldRenderAsCluster(ICluster cluster)
        {
            return cluster.Size > 1;
        }

        private global::Android.Gms.Maps.Model.BitmapDescriptor GetNativeDescriptor(StaticCluster clusterPin)
        {
            var size = GetSizeByCount(clusterPin.Size);
            var clusterViewCopy = ((IClusterView)_clusterView).Instance();
            clusterViewCopy.ChangeClusterText(GetLabelByCount(clusterPin.Size));
            clusterViewCopy.ChangeSize(size);
            var viewBMP = BitmapDescriptorFactory.FromView(() => (ContentView)clusterViewCopy);
            return _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
        }

        private static int GetSizeByCount(int count)
        {
            if (count < 10)
            {
                return 32;
            }
            else if (count > 9 && count < 100)
            {
                return 36;
            }
            else if (count > 99 && count < 1000)
            {
                return 40;
            }
            else if (count > 999 && count < 10000)
            {
                return 44;
            }
            else
            {
                return 48;
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

