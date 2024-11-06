using CoreLocation;
using Foundation;
using Google.Maps.Utils;
using Maui.GoogleMaps.Handlers;

namespace Maui.GoogleMaps.Platforms.iOS.Algorithm
{
    public class ViewBasedClusterAlgorithm : GMUNonHierarchicalDistanceBasedAlgorithm
    {
        private const double EarthRadiusKm = 6371.0;
        private readonly MapHandler _handler;
        private List<IGMUClusterItem> _clusters = new();

        public ViewBasedClusterAlgorithm(IElementHandler handler): base(new nuint(80))
        {
            _handler = (MapHandler)handler;

            _handler.VirtualView.CameraIdled += CameraIdled;
        }

        private void CameraIdled(object sender, CameraIdledEventArgs e)
        {
            _handler.clusterManager.Cluster();
        }

        public override void AddItems(IGMUClusterItem[] items)
        {
            foreach (var item in items)
            {
                _clusters.Add(item);
            }
            base.AddItems(items);
        }

        public override void RemoveItem(IGMUClusterItem item)
        {
            _clusters.Remove(item);
            base.RemoveItem(item);
        }

        public override IGMUCluster[] ClustersAtZoom(float zoom)
        {
            var visibleBounds = GetVisibleBounds();
            var vi = _clusters.Where(p => CheckPointIsWithinBounds(p.Position, visibleBounds)).ToArray();
            var visibleItems = ClusterByMinimumDistance(vi, GetClusterDistanceByZoom((int)Math.Ceiling(zoom)));
            //var items = base.ClustersAtZoom(zoom);
            //var visibleItems = items.Where(p => CheckPointIsWithinBounds(p.Position, visibleBounds)).ToArray();
            return visibleItems;
        }

        private BoundsLocationCoordinate2D GetVisibleBounds()
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

        private static bool CheckPointIsWithinBounds(CLLocationCoordinate2D point, BoundsLocationCoordinate2D bounds)
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

        public static IGMUCluster[] ClusterByMinimumDistance(IGMUClusterItem[] points, double minDistanceKm)
        {
            List<CustomClusterWrapper> clusters = new();

            foreach (var point in points)
            {
                bool assigned = false;

                foreach (var cluster in clusters)
                {
                    if (CalculateDistanceInKm(new Position(point.Position.Latitude, point.Position.Longitude), new Position(cluster.Position.Latitude, cluster.Position.Longitude)) <= minDistanceKm)
                    {
                        cluster.ItemsList.Add(point);
                        assigned = true;
                        break;
                    }

                    if (assigned)
                        break;
                }
                if (!assigned)
                {
                    var newCluster = new CustomClusterWrapper
                    {
                        Position = point.Position
                    };
                    newCluster.ItemsList.Add(point);
                    clusters.Add(newCluster);
                }
            }
            return clusters.ToArray();
        }

        public static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double latRad1 = Math.PI * lat1 / 180.0;
            double lonRad1 = Math.PI * lon1 / 180.0;
            double latRad2 = Math.PI * lat2 / 180.0;
            double lonRad2 = Math.PI * lon2 / 180.0;

            double deltaLat = latRad2 - latRad1;
            double deltaLon = lonRad2 - lonRad1;

            double a = Math.Pow(Math.Sin(deltaLat / 2), 2) +
                       Math.Cos(latRad1) * Math.Cos(latRad2) *
                       Math.Pow(Math.Sin(deltaLon / 2), 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double distance = EarthRadiusKm * c;

            return distance;
        }

        public static (double maxLat, double minLat, double maxLon, double minLon) GetBoundingBox(double lat, double lon, double distance)
        {
            double distanceRad = distance / EarthRadiusKm;

            double latRad = Math.PI * lat / 180.0;
            double lonRad = Math.PI * lon / 180.0;

            double maxLat = Math.Asin(Math.Sin(latRad) * Math.Cos(distanceRad) +
                                      Math.Cos(latRad) * Math.Sin(distanceRad));

            double minLat = Math.Asin(Math.Sin(latRad) * Math.Cos(distanceRad) -
                                      Math.Cos(latRad) * Math.Sin(distanceRad));

            double maxLon = lonRad + Math.Atan2(Math.Sin(distanceRad) * Math.Cos(latRad),
                                                Math.Cos(distanceRad) - Math.Sin(latRad) * Math.Sin(maxLat));

            double minLon = lonRad - Math.Atan2(Math.Sin(distanceRad) * Math.Cos(latRad),
                                                Math.Cos(distanceRad) - Math.Sin(latRad) * Math.Sin(minLat));

            maxLat = maxLat * 180.0 / Math.PI;
            minLat = minLat * 180.0 / Math.PI;
            maxLon = maxLon * 180.0 / Math.PI;
            minLon = minLon * 180.0 / Math.PI;

            maxLon = (maxLon + 540) % 360 - 180;
            minLon = (minLon + 540) % 360 - 180;

            return (maxLat, minLat, maxLon, minLon);
        }

        public static double CalculateDistanceInKm(Position p1, Position p2)
        {
            double latRad1 = Math.PI * p1.Latitude / 180.0;
            double lonRad1 = Math.PI * p1.Longitude / 180.0;
            double latRad2 = Math.PI * p2.Latitude / 180.0;
            double lonRad2 = Math.PI * p2.Longitude / 180.0;

            double deltaLat = latRad2 - latRad1;
            double deltaLon = lonRad2 - lonRad1;

            double a = Math.Pow(Math.Sin(deltaLat / 2), 2) +
                       Math.Cos(latRad1) * Math.Cos(latRad2) *
                       Math.Pow(Math.Sin(deltaLon / 2), 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double distance = EarthRadiusKm * c;

            return distance;
        }

        public static double GetClusterDistanceByZoom(int zoom)
        {
            return zoom switch
            {
                0 or 1 or 2 or 3 or 4 => 500,
                5 => 200,
                6 => 100,
                7 => 70,
                8 => 35,
                9 => 20,
                10 => 10,
                11 => 5,
                12 => 3,
                13 => 1,
                14 => 0.500,
                15 => 0.250,
                16 => 0.100,
                17 => 0.080,
                18 or 19 or 20 or 21 => 0.050,
                _ => throw new ArgumentOutOfRangeException(nameof(zoom), "El nivel de zoom debe estar entre 0 y 21."),
            };
        }
    }

    public class BoundsLocationCoordinate2D
    {
        public CLLocationCoordinate2D NorthWest { get; set; }
        public CLLocationCoordinate2D NorthEast { get; set; }
        public CLLocationCoordinate2D SouthWest { get; set; }
        public CLLocationCoordinate2D SouthEast { get; set; }
    }

    public class CustomClusterWrapper : NSObject, IGMUCluster
    {
        public List<IGMUClusterItem> ItemsList { get; set; } = new();

        public CLLocationCoordinate2D Position { get; set; }

        public nuint Count => nuint.Parse($"{ItemsList.Count}");

        public IGMUClusterItem[] Items => ItemsList.ToArray();
    }
}

