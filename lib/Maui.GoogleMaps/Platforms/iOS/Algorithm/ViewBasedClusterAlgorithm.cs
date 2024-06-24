using CoreLocation;
using Google.Maps.Utils;
using Maui.GoogleMaps.Handlers;

namespace Maui.GoogleMaps.Platforms.iOS.Algorithm
{
    public class ViewBasedClusterAlgorithm : NonHierarchicalDistanceBasedAlgorithm
    {
        private int _widthDp;
        private int _heightDp;
        private readonly MapHandler _handler;
        private Position? _mapCenter = null;
        private bool _shouldRecluster;

        public ViewBasedClusterAlgorithm(int widthDp, int heightDp, bool shouldReclusterOnMapMovement, IElementHandler handler)
        {
            _widthDp = widthDp;
            _heightDp = heightDp;
            _shouldRecluster = shouldReclusterOnMapMovement;
            _handler = (MapHandler)handler;

            _handler.VirtualView.CameraIdled += CameraIdled;
        }

        private void CameraIdled(object sender, CameraIdledEventArgs e)
        {
            _handler.clusterManager.Cluster();
        }

        public override void AddItems(IClusterItem[] items)
        {
            base.AddItems(items);
        }

        public override void RemoveItem(IClusterItem item)
        {
            base.RemoveItem(item);
        }

        public override ICluster[] ClustersAtZoom(float zoom)
        {
            var items = base.ClustersAtZoom(zoom);
            var visibleBounds = GetVisibleBounds(zoom);
            var visibleItems = items.Where(p => CheckPointIsWithinBounds(p.Position, visibleBounds)).ToArray();
            return visibleItems;
        }

        public void updateViewSize(int width, int height)
        {
            _widthDp = width;
            _heightDp = height;
        }

        public bool shouldReclusterOnMapMovement()
        {
            return _shouldRecluster;
        }

        public void setShouldReclusterOnMapMovement(bool shouldRecluster)
        {
            _shouldRecluster = shouldRecluster;
        }

        private BoundsLocationCoordinate2D GetVisibleBounds(float zoom)
        {
            if (_handler.VirtualView.Region == null)
            {
                return new BoundsLocationCoordinate2D();
            }

            return new BoundsLocationCoordinate2D
            {
                NorthWest = new CLLocationCoordinate2D(_handler.VirtualView.Region.FarRight.Latitude, _handler.VirtualView.Region.FarRight.Longitude),
                NorthEast = new CLLocationCoordinate2D(_handler.VirtualView.Region.FarLeft.Latitude, _handler.VirtualView.Region.FarLeft.Longitude),
                SouthWest = new CLLocationCoordinate2D(_handler.VirtualView.Region.NearRight.Latitude, _handler.VirtualView.Region.NearRight.Longitude),
                SouthEast = new CLLocationCoordinate2D(_handler.VirtualView.Region.NearLeft.Latitude, _handler.VirtualView.Region.NearLeft.Longitude),
            };
        }

        private bool CheckPointIsWithinBounds(CLLocationCoordinate2D point, BoundsLocationCoordinate2D bounds)
        {
            var isWithin = false;

            var isInNW = point.Latitude < bounds.NorthWest.Latitude && point.Longitude < bounds.NorthWest.Longitude;
            var isInNE = point.Latitude < bounds.NorthEast.Latitude && point.Longitude >= bounds.NorthEast.Longitude;
            var isInSW = point.Latitude >= bounds.SouthWest.Latitude && point.Longitude < bounds.SouthWest.Longitude;
            var isInSE = point.Latitude >= bounds.SouthEast.Latitude && point.Longitude >= bounds.SouthEast.Longitude;

            if (isInNW && isInNE && isInSW && isInSE)
            {
                isWithin = true;
            }

            return isWithin;
        }
    }

    public class BoundsLocationCoordinate2D
    {
        public CLLocationCoordinate2D NorthWest { get; set; }
        public CLLocationCoordinate2D NorthEast { get; set; }
        public CLLocationCoordinate2D SouthWest { get; set; }
        public CLLocationCoordinate2D SouthEast { get; set; }
    }
}

