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
        private readonly MapHandler _handler;
        private readonly ContentView _noClusterView;
        private readonly ContentView _clusterView;
        private readonly ContentView _labelizedView;
        private double _previousZoom = 0;

        public ClusterMarkerRenderer(ContentView labelizedView, ContentView noClusterView, ContentView clusterView, Context context, IElementHandler handler, IBitmapDescriptorFactory bitmapDescriptorFactory, GoogleMap map, ClusterManager clusterManager) : base(context, map, clusterManager)
        {
            _bitmapDescriptorFactory = bitmapDescriptorFactory;
            _handler = (MapHandler)handler;
            _noClusterView = noClusterView;
            _clusterView = clusterView;
            _labelizedView = labelizedView;
        }

        protected ClusterMarkerRenderer(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override bool ShouldRender(ICollection oldClusters, ICollection newClusters)
        {
            if (_handler.VirtualView.CameraPosition.Zoom >= _handler.VirtualView.ZoomLevelForLabeling)
            {
                _previousZoom = _handler.VirtualView.CameraPosition.Zoom;
                return true;
            }
            if (_previousZoom < _handler.VirtualView.ZoomLevelForLabeling)
            {
                _previousZoom = _handler.VirtualView.CameraPosition.Zoom;
                return true;
            }
            _previousZoom = _handler.VirtualView.CameraPosition.Zoom;
            return base.ShouldRender(oldClusters, newClusters);
        }

        protected override async void OnBeforeClusterItemRendered(Java.Lang.Object item, MarkerOptions markerOptions)
        {
            var clusterPin = (GoogleClusterPin)item;
            //await Task.Delay(25);
            if (_handler.VirtualView.CameraPosition.Zoom >= _handler.VirtualView.ZoomLevelForLabeling)
            {
                if (_labelizedView != null)
                {
                    var labelizedViewCopy = ((IClusterView)_labelizedView).Instance();
                    labelizedViewCopy.ChangeClusterText(clusterPin.Title);
                    var viewBMP = BitmapDescriptorFactory.FromView(() => (ContentView)labelizedViewCopy);
                    using var nativeMarker = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                    markerOptions.SetPosition(clusterPin.Position);
                    markerOptions.SetIcon(nativeMarker);
                    markerOptions.SetTitle(clusterPin.Title);
                    markerOptions.SetSnippet(clusterPin.Snippet);
                    markerOptions.InfoWindowAnchor(5000, 5000);
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
                    if (_handler.VirtualView.ClusteringMaxReached && _clusterView != null)
                    {
                        var clusterViewCopy = ((IClusterView)_clusterView).Instance();
                        var count = 0;
                        var splitted = clusterPin.Snippet.Split("-").ToList();
                        splitted.Remove("");
                        count += splitted.Count;
                        if (count == 1)
                        {
                            var viewBMP = BitmapDescriptorFactory.FromView(() => _noClusterView.Content);
                            using var nativeDescriptor = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                            markerOptions.SetPosition(clusterPin.Position);
                            markerOptions.SetIcon(nativeDescriptor);
                            markerOptions.SetTitle(clusterPin.Title);
                            markerOptions.SetSnippet(clusterPin.Snippet);
                            markerOptions.InfoWindowAnchor(5000, 5000);
                        }
                        else
                        {
                            var size = GetSizeByCount(count);
                            clusterViewCopy.ChangeClusterText(GetLabelByCount(count));
                            clusterViewCopy.ChangeSize(size);
                            var viewBMP = BitmapDescriptorFactory.FromView(() => (ContentView)clusterViewCopy);
                            using var nativeDescriptor = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                            markerOptions.SetPosition(clusterPin.Position);
                            markerOptions.Anchor(0.5f, 0.5f);
                            markerOptions.SetIcon(nativeDescriptor);
                            markerOptions.SetTitle(clusterPin.Title);
                            markerOptions.SetSnippet(clusterPin.Snippet);
                            markerOptions.InfoWindowAnchor(5000, 5000);
                        }
                    }
                    else
                    {
                        var viewBMP = BitmapDescriptorFactory.FromView(() => _noClusterView.Content);
                        using var nativeDescriptor = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                        markerOptions.SetPosition(clusterPin.Position);
                        markerOptions.SetIcon(nativeDescriptor);
                        markerOptions.SetTitle(clusterPin.Title);
                        markerOptions.SetSnippet(clusterPin.Snippet);
                        markerOptions.InfoWindowAnchor(5000, 5000);
                    }
                }
            }
        }

        protected override async void OnClusterItemUpdated(Java.Lang.Object item, Marker marker)
        {
            var clusterPin = (GoogleClusterPin)item;
            //await Task.Delay(25);
            if (_handler.VirtualView.CameraPosition.Zoom >= _handler.VirtualView.ZoomLevelForLabeling)
            {
                if (_labelizedView != null)
                {
                    var labelizedViewCopy = ((IClusterView)_labelizedView).Instance();
                    labelizedViewCopy.ChangeClusterText(clusterPin.Title);
                    var viewBMP = BitmapDescriptorFactory.FromView(() => (ContentView)labelizedViewCopy);
                    using var nativeMarker = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                    marker.Position = clusterPin.Position;
                    marker.SetIcon(nativeMarker);
                    marker.Title = clusterPin.Title;
                    marker.Snippet = clusterPin.Snippet;
                    marker.HideInfoWindow();
                    return;
                }
            }
            if (_noClusterView == null)
            {
                base.OnClusterItemUpdated(item, marker);
            }
            else
            {

                if (_handler.VirtualView.ClusteringMaxReached && _clusterView != null)
                {
                    var clusterViewCopy = ((IClusterView)_clusterView).Instance();
                    var count = 0;
                    var splitted = clusterPin.Snippet.Split("-").ToList();
                    splitted.Remove("");
                    count += splitted.Count;
                    if (count == 1)
                    {
                        var viewBMP = BitmapDescriptorFactory.FromView(() => _noClusterView.Content);
                        using var nativeDescriptor = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                        marker.Position = clusterPin.Position;
                        marker.SetIcon(nativeDescriptor);
                        marker.Title = clusterPin.Title;
                        marker.Snippet = clusterPin.Snippet;
                        marker.HideInfoWindow();
                    }
                    else
                    {
                        var size = GetSizeByCount(count);
                        clusterViewCopy.ChangeClusterText(GetLabelByCount(count));
                        clusterViewCopy.ChangeSize(size);
                        var viewBMP = BitmapDescriptorFactory.FromView(() => (ContentView)clusterViewCopy);
                        using var nativeDescriptor = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                        marker.Position = clusterPin.Position;
                        marker.SetIcon(nativeDescriptor);
                        marker.Title = clusterPin.Title;
                        marker.Snippet = clusterPin.Snippet;
                        marker.HideInfoWindow();
                    }
                }
                else
                {
                    var viewBMP = BitmapDescriptorFactory.FromView(() => _noClusterView.Content);
                    using var nativeDescriptor = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                    marker.Position = clusterPin.Position;
                    marker.SetIcon(nativeDescriptor);
                    marker.Title = clusterPin.Title;
                    marker.Snippet = clusterPin.Snippet;
                    marker.HideInfoWindow();
                }
            }
        }

        protected override async void OnBeforeClusterRendered(ICluster cluster, MarkerOptions markerOptions)
        {
            var clusterPin = (StaticCluster)cluster;
            //await Task.Delay(25);
            if (_clusterView == null)
            {
                base.OnBeforeClusterRendered(cluster, markerOptions);
            }
            else
            {
                try
                {
                    var nativeDescriptor = GetNativeDescriptor(clusterPin);
                    markerOptions.SetPosition(clusterPin.Position);
                    markerOptions.Anchor(0.5f, 0.5f);
                    markerOptions.SetIcon(nativeDescriptor);
                    markerOptions.InfoWindowAnchor(5000, 5000);

                    var title = "";
                    foreach (var c in cluster.Items)
                    {
                        title += $"{((GoogleClusterPin)c).Snippet}-";
                    }
                    markerOptions.SetSnippet(title);
                }
                catch
                {
                    var viewBMP = BitmapDescriptorFactory.FromView(() => _noClusterView.Content);
                    using var nativeDescriptor = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                    markerOptions.SetIcon(nativeDescriptor);
                }
            }
        }

        protected override async void OnClusterUpdated(ICluster cluster, Marker marker)
        {
            var clusterPin = (StaticCluster)cluster;
            //await Task.Delay(25);
            if (_clusterView == null)
            {
                base.OnClusterUpdated(cluster, marker);
            }
            else
            {
                try
                {
                    var nativeDescriptor = GetNativeDescriptor(clusterPin);
                    marker.Position = clusterPin.Position;
                    marker.HideInfoWindow();
                    marker.SetIcon(nativeDescriptor);
                    var title = "";
                    foreach (var c in cluster.Items)
                    {
                        title += $"{((GoogleClusterPin)c).Snippet}-";
                    }
                    marker.Snippet = title;
                }
                catch
                {
                    var viewBMP = BitmapDescriptorFactory.FromView(() => _noClusterView.Content);
                    using var nativeDescriptor = _bitmapDescriptorFactory.ToNative(viewBMP, _handler.MauiContext);
                    marker.Position = clusterPin.Position;
                    marker.SetIcon(nativeDescriptor);
                }
            }
        }

        protected override bool ShouldRenderAsCluster(ICluster cluster)
        {
            //if (_handler.VirtualView.CameraPosition.Zoom >= _handler.VirtualView.ZoomLevelForLabeling)
            //{
            //    return false;
            //}
            return cluster.Size > 1;
        }

        private global::Android.Gms.Maps.Model.BitmapDescriptor GetNativeDescriptor(StaticCluster clusterPin)
        {
            var clusterViewCopy = ((IClusterView)_clusterView).Instance();
            if (_handler.VirtualView.ClusteringMaxReached)
            {
                var count = 0;
                foreach (GoogleClusterPin c in clusterPin.Items)
                {
                    var splitted = c.Snippet.Split("-").ToList();
                    splitted.Remove("");
                    count += splitted.Count;
                }
                var size = GetSizeByCount(count);
                clusterViewCopy.ChangeClusterText(GetLabelByCount(count));
                clusterViewCopy.ChangeSize(size);
            }
            else
            {
                var size = GetSizeByCount(clusterPin.Size);
                clusterViewCopy.ChangeClusterText(GetLabelByCount(clusterPin.Size));
                clusterViewCopy.ChangeSize(size);
            }
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
            else if (count > 999 && count < 5000)
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
            else if (count > 1000 && count < 5001)
            {
                return "1000+";
            }
            else
            {
                return "5000+";
            }
        }
    }
}

