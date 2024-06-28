using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Runtime;
using Com.Google.Maps.Android.Collections;

namespace Maui.GoogleMaps.Platforms.Android.Renderers
{
    public class CustomMarkerManager : MarkerManager
    {
        private readonly Map _virtualView;
        protected CustomMarkerManager(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public CustomMarkerManager(GoogleMap map, Map virtualView) : base(map)
        {
            _virtualView = virtualView;
        }

        public override bool OnMarkerClick(Marker marker)
        {
            _virtualView.SelectedCluster = new ClusterPin
            {
                Title = marker.Title,
                Snippet = marker.Snippet,
                Position = new Position(marker.Position.Latitude, marker.Position.Longitude),
                NativeObject = marker
            };
            return base.OnMarkerClick(marker);
        }
    }
}

