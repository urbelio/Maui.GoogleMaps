using System.Diagnostics.Metrics;
using Google.Maps;
using Google.Maps.Utils;
using Maui.GoogleMaps.Handlers;
using Maui.GoogleMaps.iOS.Factories;
using Maui.GoogleMaps.Platforms.iOS.Logics;
using Maui.GoogleMaps.Views;

namespace Maui.GoogleMaps.Platforms.iOS.Renderers
{
    public class ClusterMarkerRenderer : GMUDefaultClusterRenderer
    {
        private readonly IImageFactory _imageFactory;
        private readonly MapHandler _handler;
        public ClusterMarkerRenderer(IElementHandler handler, IImageFactory imageFactory, MapView map, IGMUClusterIconGenerator clusterIconGenerator) : base(map, clusterIconGenerator)
        {
            _imageFactory = imageFactory;
            _handler = (MapHandler)handler;
        }

        public override void RenderClusters(IGMUCluster[] clusters)
        {
            base.RenderClusters(clusters);
            var zoom = _handler.VirtualView.CameraPosition.Zoom;
            if (zoom >= _handler.VirtualView.ZoomLevelForLabeling)
            {
                foreach (var m in base.Markers)
                {
                    var cl = clusters.Where(p => p.Position.Equals(m.Position)).First();
                    if (_handler.VirtualView.LabelizedView != null)
                    {
                        try
                        {
                            if (cl.Count == 1)
                            {
                                ((IClusterView)_handler.VirtualView.LabelizedView).ChangeClusterText(((GoogleClusterPin)cl.Items.First()).Title);
                                var bmp = BitmapDescriptorFactory.FromView(() => _handler.VirtualView.LabelizedView);
                                var nativeDescriptor = _imageFactory.ToUIImage(bmp, _handler.MauiContext);
                                if (nativeDescriptor != null)
                                {
                                    m.Icon = nativeDescriptor;
                                }
                            }
                        }
                        catch { }
                    }
                    var title = "";
                    foreach (GoogleClusterPin c in cl.Items.Cast<GoogleClusterPin>())
                    {
                        title += $"{c.Snippet}-";
                    }
                    m.Snippet = title;
                    m.InfoWindowAnchor = new CoreGraphics.CGPoint(5000, 6000);
                }
            }
            else
            {
                foreach (var m in base.Markers)
                {
                    var cl = clusters.Where(p => p.Position.Equals(m.Position)).First();
                    if (_handler.VirtualView.NoClusterView != null)
                    {
                        try
                        {
                            if (cl.Count == 1)
                            {
                                var bmp = BitmapDescriptorFactory.FromView(() => _handler.VirtualView.NoClusterView);
                                var nativeDescriptor = _imageFactory.ToUIImage(bmp, _handler.MauiContext);
                                if (nativeDescriptor != null)
                                {
                                    m.Icon = nativeDescriptor;
                                }
                            }
                        }
                        catch { }
                    }
                    var title = "";
                    foreach (GoogleClusterPin c in cl.Items.Cast<GoogleClusterPin>())
                    {
                        title += $"{c.Snippet}-";
                    }
                    m.Snippet = title;
                    m.InfoWindowAnchor = new CoreGraphics.CGPoint(5000, 6000);
                }
            }
        }

        public override bool ShouldRenderAsCluster(IGMUCluster cluster, float zoom)
        {
            if (_handler.VirtualView.ClusteringEnabled && _handler.VirtualView.LabelizedView != null)
            {
                if (_handler.VirtualView.CameraPosition.Zoom >= _handler.VirtualView.ZoomLevelForLabeling)
                {
                    return true;
                }
                return true;
            }
            else
            {
                return cluster.Count > 1;
            }
        }
    }
}

