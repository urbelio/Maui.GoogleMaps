using System.Collections;
using System.Diagnostics;
using Android.Gms.Maps.Model;
using Android.Runtime;
using Com.Google.Maps.Android.Clustering.Algo;
using Maui.GoogleMaps.Handlers;
using Maui.GoogleMaps.Logics.Android;

namespace Maui.GoogleMaps.Platforms.Android.Algorithm
{
    public class NonHierarchicalViewBasedAlgorithmExtended : NonHierarchicalViewBasedAlgorithm
    {
        private const double EarthRadiusKm = 6371.0;
        private readonly MapHandler _handler;
        private readonly List<GoogleClusterPin> _clusters = new();

        public NonHierarchicalViewBasedAlgorithmExtended(IElementHandler handler, int widthDp, int heightDp) : base(widthDp, heightDp)
        {
            _handler = (MapHandler)handler;
        }

        protected NonHierarchicalViewBasedAlgorithmExtended(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        //protected override ICollection GetClusteringItems(PointQuadTree quadTree, float zoom)
        //{
        //    var clusters = base.GetClusteringItems(quadTree, zoom);
        //    return clusters;
        //}

        public override bool ShouldReclusterOnMapMovement()
        {
            Debug.WriteLine("..........................Comprobando movimiento en NonHierarchicalViewBasedAlgorithmExtended.......................");
            return true;
        }

        public override bool AddItem(Java.Lang.Object item)
        {
            //Debug.Write("..........................Añadiendo punto en NonHierarchicalViewBasedAlgorithmExtended.......................");
            _clusters.Add((GoogleClusterPin)item);
            return true;
            //var value = base.AddItem(item);
            //return value;
        }

        public override bool AddItems(ICollection items)
        {
            Debug.WriteLine("..........................Añadiendo puntos en NonHierarchicalViewBasedAlgorithmExtended.......................");
            foreach (var item in items)
            {
                _clusters.Add((GoogleClusterPin)item);
            }
            return true;
            //var value = base.AddItems(items);
            //return value;
        }

        public override void ClearItems()
        {
            if (_clusters.Count > 0)
            {
                Debug.WriteLine("..........................Limpiando puntos en NonHierarchicalViewBasedAlgorithmExtended.......................");
                _clusters.Clear();
                //base.ClearItems();
            }
        }

        public override ICollection GetClusters(float zoom)
        {
            //var clusters = base.GetClusters(zoom);
            //return clusters;

            Debug.WriteLine("..........................Obteniendo puntos en NonHierarchicalViewBasedAlgorithmExtended.......................");
            var visibleBounds = GetVisibleBounds();
            var vi = _clusters.Where(p => CheckPointIsWithinBounds(p.Position, visibleBounds)).ToArray();
            var visibleItems = ClusterByMinimumDistance(vi, GetClusterDistanceByZoom((int)Math.Ceiling(zoom)));

            return visibleItems;
        }

        private static bool CheckPointIsWithinBounds(LatLng position, MapRegion visibleBounds)
        {
            var isWithin = false;

            var isInNW = position.Latitude < visibleBounds.FarRight.Latitude && position.Longitude < visibleBounds.FarRight.Longitude;
            var isInNE = position.Latitude < visibleBounds.FarLeft.Latitude && position.Longitude >= visibleBounds.FarLeft.Longitude;
            var isInSW = position.Latitude >= visibleBounds.NearRight.Latitude && position.Longitude < visibleBounds.NearRight.Longitude;
            var isInSE = position.Latitude >= visibleBounds.NearLeft.Latitude && position.Longitude >= visibleBounds.NearLeft.Longitude;

            if (isInNW && isInNE && isInSW && isInSE)
            {
                isWithin = true;
            }

            return isWithin;
        }

        private MapRegion GetVisibleBounds()
        {
            if (_handler.VirtualView.Region != null)
            {
                return _handler.VirtualView.Region;
            }
            else
            {
                return new MapRegion(new Position(0,0), new Position(0, 0), new Position(0, 0), new Position(0, 0));
            }
        }

        public static StaticCluster[] ClusterByMinimumDistance(GoogleClusterPin[] points, double minDistanceKm)
        {
            List<CustomStaticCluster> clusters = new();

            foreach (var point in points)
            {
                bool assigned = false;

                foreach (var cluster in clusters)
                {
                    if (CalculateDistanceInKm(new Position(point.Position.Latitude, point.Position.Longitude), new Position(cluster.Position.Latitude, cluster.Position.Longitude)) <= minDistanceKm)
                    {
                        cluster.Points.Add(point);
                        assigned = true;
                        break;
                    }

                    if (assigned)
                        break;
                }
                if (!assigned)
                {
                    var newCluster = new CustomStaticCluster(new LatLng(point.Position.Latitude, point.Position.Longitude));
                    newCluster.Points.Add(point);
                    clusters.Add(newCluster);
                }
            }
            return clusters.ToArray();
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
                5 => 250,
                6 => 140,
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

    public class CustomStaticCluster : StaticCluster
    {
        public List<GoogleClusterPin> Points { get; set; } = new ();

        protected CustomStaticCluster(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public CustomStaticCluster(LatLng center) : base(center)
        {

        }

        public override ICollection Items => Points;

        public override int Size => Points.Count;
    }
}

