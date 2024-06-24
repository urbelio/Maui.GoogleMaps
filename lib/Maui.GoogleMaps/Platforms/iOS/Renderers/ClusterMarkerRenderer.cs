using Google.Maps;
using Google.Maps.Utils;
using Maui.GoogleMaps.Handlers;
using Maui.GoogleMaps.iOS.Factories;
using Maui.GoogleMaps.Platforms.iOS.Logics;
using Maui.GoogleMaps.Views;

namespace Maui.GoogleMaps.Platforms.iOS.Renderers
{
    public class ClusterMarkerRenderer : DefaultClusterRenderer
    {
        private readonly IImageFactory _imageFactory;
        private readonly MapHandler _handler;
        public ClusterMarkerRenderer(IElementHandler handler, IImageFactory imageFactory, MapView map, IClusterIconGenerator clusterIconGenerator) : base(map, clusterIconGenerator)
        {
            _imageFactory = imageFactory;
            _handler = (MapHandler)handler;
        }

        public override void RenderClusters(ICluster[] clusters)
        {
            base.RenderClusters(clusters);
            var zoom = _handler.VirtualView.CameraPosition.Zoom;
            if (zoom > 17)
            {
                foreach (var m in base.Markers)
                {
                    if (_handler.VirtualView.LabelizedView != null)
                    {
                        try
                        {
                            var cl = clusters.Where(p => p.Position.Equals(m.Position)).First();
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
                }
            }
            else
            {
                foreach (var m in base.Markers)
                {
                    if (_handler.VirtualView.NoClusterView != null)
                    {
                        try
                        {
                            var cl = clusters.Where(p => p.Position.Equals(m.Position)).First();
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
                }
            }
        }

        public override bool ShouldRenderAsCluster(ICluster cluster, float zoom)
        {
            if (_handler.VirtualView.ClusteringEnabled && _handler.VirtualView.LabelizedView != null)
            {
                return true;
            }
            else
            {
                return cluster.Count > 1;
            }
        }
    }
}

