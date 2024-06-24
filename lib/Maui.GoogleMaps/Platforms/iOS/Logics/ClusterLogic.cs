using CoreLocation;
using Google.Maps;
using Google.Maps.Utils;
using Maui.GoogleMaps.Handlers;
using Maui.GoogleMaps.iOS.Factories;
using Maui.GoogleMaps.Logics;
using System.Collections;

namespace Maui.GoogleMaps.Platforms.iOS.Logics
{
    internal class ClusterLogic : DefaultClusterLogic<GoogleClusterPin, MapView>
    {
        private readonly IImageFactory _imageFactory;

        public ClusterLogic(IImageFactory imageFactory)
        {
            _imageFactory = imageFactory;
        }

        internal override void Register(MapView oldNativeMap, Map oldMap, MapView newNativeMap, Map newMap, IElementHandler handler)
        {
            base.Register(oldNativeMap, oldMap, newNativeMap, newMap, handler);
        }

        internal override void Unregister(MapView nativeMap, Map map)
        {
            base.Unregister(nativeMap, map);
        }

        protected override IList<ClusterPin> GetItems(Map map)
        {
            var list = new List<ClusterPin>();
            foreach (var item in map.ItemsSource)
            {
                var cp = (ClusterPin)item;
                list.Add(cp);
            }
            return list;
        }

        protected override void AddItems(IList newItems)
        {
            foreach (ClusterPin p in newItems)
            {
                CheckCanCreateNativeItem(p);
                p.PropertyChanged += OnItemPropertyChanged;
                var nat = CreateNativeItem(p);
            }
            ((MapHandler)Map.Handler).clusterManager?.Cluster();
        }

        protected override void RemoveItems(IList oldItems)
        {
            foreach (ClusterPin p in oldItems)
            {
                DeleteNativeItem(p);
            }
            ((MapHandler)Map.Handler).clusterManager?.Cluster();
        }

        protected override GoogleClusterPin CreateNativeItem(ClusterPin outerItem)
        {
            //UIImage nativeDescriptor = null;
            //try
            //{
            //    nativeDescriptor = _imageFactory.ToUIImage(outerItem.GetIcon(), Handler.MauiContext);
            //}
            //catch { }
            var point = new GoogleClusterPin(outerItem.GetPosition().Latitude, outerItem.GetPosition().Longitude, outerItem.GetTitle(), outerItem.GetSnippet());
            ((MapHandler)Map.Handler).clusterManager?.AddItem(point);
            return point;
        }

        protected override GoogleClusterPin DeleteNativeItem(ClusterPin outerItem)
        {
            //UIImage nativeDescriptor = null;
            //try
            //{
            //    nativeDescriptor = _imageFactory.ToUIImage(outerItem.GetIcon(), Handler.MauiContext);
            //}
            //catch { }
            var point = new GoogleClusterPin(outerItem.GetPosition().Latitude, outerItem.GetPosition().Longitude, outerItem.GetTitle(), outerItem.GetSnippet());
            ((MapHandler)Map.Handler).clusterManager?.RemoveItem(point);
            return point;
        }

        protected override void OnUpdateTitle(ClusterPin outerItem, GoogleClusterPin nativeItem)
        {
            nativeItem.Title = outerItem.Title;
        }

        protected override void OnUpdateSnippet(ClusterPin outerItem, GoogleClusterPin nativeItem)
        {
            nativeItem.Snippet = outerItem.Snippet;
        }

        protected override void OnUpdatePosition(ClusterPin outerItem, GoogleClusterPin nativeItem)
        {
            nativeItem.PinPosition = new CLLocationCoordinate2D(outerItem.Position.Latitude, outerItem.Position.Longitude);
        }
    }

    public class GoogleClusterPin : ClusterItem
    {
        public override CLLocationCoordinate2D Position => PinPosition;

        public CLLocationCoordinate2D PinPosition { get; set; }

        public string Snippet { get; set; }

        public string Title { get; set; }

        public GoogleClusterPin(double lat, double lng, string title, string snippet) : base()
        {
            PinPosition = new CLLocationCoordinate2D(lat, lng);
            Snippet = snippet;
            Title = title;
        }
    }
}

