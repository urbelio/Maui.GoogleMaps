using System;
using CoreLocation;
using Foundation;
using Google.Maps;
using Google.Maps.Utils;

namespace Maui.GoogleMaps.Platforms.iOS.Renderers
{
    public class CustomClusterManager : GMUClusterManager
    {
        public CustomClusterManager(NSObject mapView, IGMUClusterAlgorithm algorithm, IGMUClusterRenderer renderer) : base((Google.Maps.MapView)mapView, algorithm, renderer)
        {
        }
    }

    public class CustomMapViewDelegate : MapViewDelegate
    {
        private readonly Map _virtualView;
        public CustomMapViewDelegate(Map virtualView) : base()
        {
            _virtualView = virtualView;
        }

        public override void DidChangeCameraPosition(MapView mapView, Google.Maps.CameraPosition position)
        {
            base.DidChangeCameraPosition(mapView, position);
        }

        public override void DidTapAtCoordinate(MapView mapView, CLLocationCoordinate2D coordinate)
        {
            base.DidTapAtCoordinate(mapView, coordinate);
        }

        //public override bool TappedMarker(MapView mapView, Marker marker)
        //{
        //    _virtualView.SelectedCluster = marker.Snippet;
        //    return base.TappedMarker(mapView, marker);
        //}
    }
}

