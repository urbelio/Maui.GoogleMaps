using Android.Gms.Maps.Model;
using Android.Gms.Maps;
using Maui.GoogleMaps.Android;
using Maui.GoogleMaps.Android.Factories;
using Maui.GoogleMaps.Handlers;
using System.Collections;
using System.Diagnostics;
using Android.Gms.Maps.Utils.Clustering;
using Java.Lang;

namespace Maui.GoogleMaps.Logics.Android;

internal class ClusterLogic : DefaultClusterLogic<GoogleClusterPin, GoogleMap>
{
    private volatile bool _withoutUpdateNative = false;

    private readonly IBitmapDescriptorFactory _bitmapDescriptorFactory;

    public ClusterLogic(
        IBitmapDescriptorFactory bitmapDescriptorFactory)
    {
        _bitmapDescriptorFactory = bitmapDescriptorFactory;
    }

    internal override void Register(GoogleMap oldNativeMap, Map oldMap, GoogleMap newNativeMap, Map newMap, IElementHandler handler)
    {
        base.Register(oldNativeMap, oldMap, newNativeMap, newMap, handler);

        if (newNativeMap != null)
        {
            newNativeMap.InfoWindowClick += OnInfoWindowClick;
            newNativeMap.InfoWindowLongClick += OnInfoWindowLongClick;
            newNativeMap.MarkerClick += OnMakerClick;
            newNativeMap.InfoWindowClose += OnInfoWindowClose;
            newNativeMap.MarkerDragStart += OnMarkerDragStart;
            newNativeMap.MarkerDragEnd += OnMarkerDragEnd;
            newNativeMap.MarkerDrag += OnMarkerDrag;
        }
    }

    internal override void Unregister(GoogleMap nativeMap, Map map)
    {
        if (nativeMap != null)
        {
            nativeMap.MarkerDrag -= OnMarkerDrag;
            nativeMap.MarkerDragEnd -= OnMarkerDragEnd;
            nativeMap.MarkerDragStart -= OnMarkerDragStart;
            nativeMap.MarkerClick -= OnMakerClick;
            nativeMap.InfoWindowClose -= OnInfoWindowClose;
            nativeMap.InfoWindowClick -= OnInfoWindowClick;
            nativeMap.InfoWindowLongClick -= OnInfoWindowLongClick;
        }

        base.Unregister(nativeMap, map);
    }

    protected override IList<ClusterPin> GetItems(Map map) => map.Clusters;

    protected override void AddItems(IList newItems)
    {
        base.AddItems(newItems);
        ((MapHandler)Map.Handler).clusterManager?.Cluster();
    }

    protected override void RemoveItems(IList oldItems)
    {
        base.RemoveItems(oldItems);
        ((MapHandler)Map.Handler).clusterManager?.Cluster();
    }

    protected override GoogleClusterPin CreateNativeItem(ClusterPin outerItem)
    {
        var point = new GoogleClusterPin(outerItem.GetPosition().Latitude, outerItem.GetPosition().Longitude, outerItem.GetTitle(), outerItem.GetSnippet());
        
        ((MapHandler)Map.Handler).clusterManager?.AddItem(point);
        outerItem.NativeObject = point;
        return point;
    }

    protected override GoogleClusterPin DeleteNativeItem(ClusterPin outerItem)
    {
        if (outerItem.NativeObject is not GoogleClusterPin marker)
        {
            return null;
        }
        if (((GoogleClusterPin)outerItem.NativeObject).Position == null)
        {
            return null;
        }
        ((MapHandler)Map.Handler).clusterManager?.ClearItems();
        return null;
    }

    private ClusterPin LookupPin(GoogleClusterPin marker)
    {
        return GetItems(Map).FirstOrDefault(outerItem => ((ClusterPin)outerItem).Title == marker.Title);
    }

    private void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
    {
        Debug.WriteLine("************OnInfoWindowClick***************");
    }

    private void OnInfoWindowLongClick(object sender, GoogleMap.InfoWindowLongClickEventArgs e)
    {

        Debug.WriteLine("************OnInfoWindowLongClick***************");
    }

    private void OnMakerClick(object sender, GoogleMap.MarkerClickEventArgs e)
    {
        Debug.WriteLine("************OnMakerClick***************");
    }

    private void OnInfoWindowClose(object sender, GoogleMap.InfoWindowCloseEventArgs e)
    {
        Debug.WriteLine("************OnInfoWindowClose***************");
    }

    private void OnMarkerDragStart(object sender, GoogleMap.MarkerDragStartEventArgs e)
    {
    }

    private void OnMarkerDrag(object sender, GoogleMap.MarkerDragEventArgs e)
    {
    }

    private void OnMarkerDragEnd(object sender, GoogleMap.MarkerDragEndEventArgs e)
    {
    }

    internal override void OnMapPropertyChanged(string propertyName)
    {
        if (propertyName == Map.SelectedPinProperty.PropertyName)
        {
        }
    }

    private void UpdateSelectedPin(ClusterPin cluster)
    {
        if (cluster == null)
        {
        }
        else
        {
            if (cluster.NativeObject is GoogleClusterPin marker) 
            {
                var targetPin = LookupPin(marker);
            }
        }
    }

    private void UpdatePositionWithoutMove(ClusterPin cluster, Position position)
    {
        try
        {
            _withoutUpdateNative = true;
            cluster.Position = position;
        }
        finally
        {
            _withoutUpdateNative = false;
        }
    }

    protected override void OnUpdateSnippet(ClusterPin outerItem, GoogleClusterPin nativeItem)
        => nativeItem.Snippet = outerItem.Snippet;

    protected override void OnUpdateTitle(ClusterPin outerItem, GoogleClusterPin nativeItem)
        => nativeItem.Title = outerItem.Title;

    protected override void OnUpdatePosition(ClusterPin outerItem, GoogleClusterPin nativeItem)
    {
        if (!_withoutUpdateNative)
        {
            nativeItem.Position = outerItem.Position.ToLatLng();
        }
    }
}

public class GoogleClusterPin : Java.Lang.Object, IClusterItem
{
    public LatLng Position { get; set; }

    public string Snippet { get; set; }

    public string Title { get; set; }

    public Float ZIndex => (Float)0f;

    public GoogleClusterPin(double lat, double lng, string title, string snippet)
    {
        Position = new LatLng(lat, lng);
        Title = title;
        Snippet = snippet;
    }
}