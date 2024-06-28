using Android.Gms.Maps.Model;
using Android.Gms.Maps;
using Maui.GoogleMaps.Android;
using Maui.GoogleMaps.Android.Factories;
using Com.Google.Maps.Android.Clustering;
using Maui.GoogleMaps.Handlers;
using System.Collections;
using System.Diagnostics;

namespace Maui.GoogleMaps.Logics.Android;

internal class ClusterLogic : DefaultClusterLogic<GoogleClusterPin, GoogleMap>
{
    private volatile bool _onMarkerEvent = false;
    //private Pin _draggingPin;
    private volatile bool _withoutUpdateNative = false;

    private readonly IBitmapDescriptorFactory _bitmapDescriptorFactory;
    //private readonly Action<Pin, MarkerOptions> _onMarkerCreating;
    //private readonly Action<Pin, Marker> _onMarkerCreated;
    //private readonly Action<Pin, Marker> _onMarkerDeleting;
    //private readonly Action<Pin, Marker> _onMarkerDeleted;

    public ClusterLogic(
        IBitmapDescriptorFactory bitmapDescriptorFactory/*,
        Action<Pin, MarkerOptions> onMarkerCreating,
        Action<Pin, Marker> onMarkerCreated, 
        Action<Pin, Marker> onMarkerDeleting,
        Action<Pin, Marker> onMarkerDeleted*/)
    {
        _bitmapDescriptorFactory = bitmapDescriptorFactory;
        /*_onMarkerCreating = onMarkerCreating;
        _onMarkerCreated = onMarkerCreated;
        _onMarkerDeleting = onMarkerDeleting;
        _onMarkerDeleted = onMarkerDeleted;*/
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
        //AndroidBitmapDescriptor nativeDescriptor = null;
        //try
        //{
        //    nativeDescriptor = _bitmapDescriptorFactory.ToNative(outerItem.GetIcon(), Handler.MauiContext);
        //}
        //catch { }
        var point = new GoogleClusterPin(outerItem.GetPosition().Latitude, outerItem.GetPosition().Longitude, outerItem.GetTitle(), outerItem.GetSnippet());
        
        ((MapHandler)Map.Handler).clusterManager?.AddItem(point);
        outerItem.NativeObject = point;
        return point;
        //var markerOptions = new MarkerOptions()
        //    .SetPosition(outerItem.Position.ToLatLng())
        //    .SetTitle(outerItem.Label)
        //    .SetSnippet(outerItem.Address)
        //    .SetSnippet(outerItem.Address)
        //    .Draggable(outerItem.IsDraggable)
        //    .SetRotation(outerItem.Rotation)
        //    .Anchor((float)outerItem.Anchor.X, (float)outerItem.Anchor.Y)
        //    .InvokeZIndex(outerItem.ZIndex)
        //    .Flat(outerItem.Flat)
        //    .SetAlpha(1f - outerItem.Transparency);

        //if (outerItem.Icon != null)
        //{
        //    var nativeDescriptor = _bitmapDescriptorFactory.ToNative(outerItem.Icon, Handler.MauiContext);
        //    markerOptions.SetIcon(nativeDescriptor);
        //}

        //_onMarkerCreating(outerItem, markerOptions);

        //var marker = NativeMap.AddMarker(markerOptions);
        //// If the pin has an IconView set this method will convert it into an icon for the marker
        //marker.Visible = outerItem.IsVisible;

        //// associate pin with marker for later lookup in event handlers
        //outerItem.NativeObject = marker;
        //_onMarkerCreated(outerItem, marker);
        //return marker;
    }

    protected override GoogleClusterPin DeleteNativeItem(ClusterPin outerItem)
    {
        if (outerItem.NativeObject is not GoogleClusterPin marker)
        {
            return null;
        }
        //AndroidBitmapDescriptor nativeDescriptor = null;
        //try
        //{
        //    nativeDescriptor = _bitmapDescriptorFactory.ToNative(outerItem.GetIcon(), Handler.MauiContext);
        //}
        //catch { }
        //var point = new GoogleClusterPin(outerItem.GetPosition().Latitude, outerItem.GetPosition().Longitude, outerItem.GetTitle(), outerItem.GetSnippet());
        ((MapHandler)Map.Handler).clusterManager?.RemoveItem((GoogleClusterPin)outerItem.NativeObject);
        //_onMarkerDeleting(outerItem, marker);
        //marker.Remove();
        //outerItem.NativeObject = null;

        //if (ReferenceEquals(Map.SelectedPin, outerItem))
        //{
        //    Map.SelectedPin = null;
        //}

        //_onMarkerDeleted(outerItem, marker);
        return (GoogleClusterPin)outerItem.NativeObject;
        //return point;
    }

    private ClusterPin LookupPin(GoogleClusterPin marker)
    {
        return GetItems(Map).FirstOrDefault(outerItem => ((ClusterPin)outerItem).Title == marker.Title);
    }

    private void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
    {
        Debug.WriteLine("************OnInfoWindowClick***************");
        // lookup pin
        //var targetPin = LookupPin(e.Marker);

        // only consider event handled if a handler is present.
        // Else allow default behavior of displaying an info window.
        //targetPin?.SendTap();

        //if (targetPin != null)
        //{
        //    Map.SendInfoWindowClicked(targetPin);
        //}
    }

    private void OnInfoWindowLongClick(object sender, GoogleMap.InfoWindowLongClickEventArgs e)
    {

        Debug.WriteLine("************OnInfoWindowLongClick***************");
        // lookup pin
        //var targetPin = LookupPin(e.Marker);

        // only consider event handled if a handler is present.
        // Else allow default behavior of displaying an info window.
        //if (targetPin != null)
        //{
        //    Map.SendInfoWindowLongClicked(targetPin);
        //}
    }

    private void OnMakerClick(object sender, GoogleMap.MarkerClickEventArgs e)
    {
        Debug.WriteLine("************OnMakerClick***************");
        //// lookup pin
        //var targetPin = LookupPin(e.Marker);

        //// If set to PinClickedEventArgs.Handled = true in app codes,
        //// then all pin selection controlling by app.
        //if (Map.SendPinClicked(targetPin))
        //{
        //    e.Handled = true;
        //    return;
        //}

        //try
        //{
        //    _onMarkerEvent = true;
        //    if (targetPin != null && !ReferenceEquals(targetPin, Map.SelectedPin))
        //    {
        //        Map.SelectedPin = targetPin;
        //    }
        //}
        //finally
        //{
        //    _onMarkerEvent = false;
        //}

        //e.Handled = false;
    }

    private void OnInfoWindowClose(object sender, GoogleMap.InfoWindowCloseEventArgs e)
    {
        Debug.WriteLine("************OnInfoWindowClose***************");
        //// lookup pin
        //var targetPin = LookupPin(e.Marker);

        //try
        //{
        //    _onMarkerEvent = true;
        //    if (targetPin != null && ReferenceEquals(targetPin, Map.SelectedPin))
        //    {
        //        Map.SelectedPin = null;
        //    }
        //}
        //finally
        //{
        //    _onMarkerEvent = false;
        //}
    }

    private void OnMarkerDragStart(object sender, GoogleMap.MarkerDragStartEventArgs e)
    {
        //// lookup pin
        //_draggingPin = LookupPin(e.Marker);

        //if (_draggingPin != null)
        //{
        //    UpdatePositionWithoutMove(_draggingPin, e.Marker.Position.ToPosition());
        //    Map.SendPinDragStart(_draggingPin);
        //}
    }

    private void OnMarkerDrag(object sender, GoogleMap.MarkerDragEventArgs e)
    {
        //if (_draggingPin != null)
        //{
        //    UpdatePositionWithoutMove(_draggingPin, e.Marker.Position.ToPosition());
        //    Map.SendPinDragging(_draggingPin);
        //}
    }

    private void OnMarkerDragEnd(object sender, GoogleMap.MarkerDragEndEventArgs e)
    {
        //if (_draggingPin != null)
        //{
        //    UpdatePositionWithoutMove(_draggingPin, e.Marker.Position.ToPosition());
        //    Map.SendPinDragEnd(_draggingPin);
        //    _draggingPin = null;
        //}

        //_withoutUpdateNative = false;
    }

    internal override void OnMapPropertyChanged(string propertyName)
    {
        if (propertyName == Map.SelectedPinProperty.PropertyName)
        {
            //if (!_onMarkerEvent)
            //    UpdateSelectedPin(Map.SelectedCluster);
            //Map.SendSelectedClusterChanged(Map.SelectedCluster);
        }
    }

    private void UpdateSelectedPin(ClusterPin cluster)
    {
        if (cluster == null)
        {
            //foreach (var outerItem in GetItems(Map))
            //{
            //    (outerItem.NativeObject as GoogleClusterPin)?.HideInfoWindow();
            //}
        }
        else
        {
            // lookup pin
            if (cluster.NativeObject is GoogleClusterPin marker) 
            {
                var targetPin = LookupPin(marker);
                //(targetPin?.NativeObject as GoogleClusterPin)?.ShowInfoWindow();
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

    public GoogleClusterPin(double lat, double lng, string title, string snippet)
    {
        Position = new LatLng(lat, lng);
        Title = title;
        Snippet = snippet;
    }
}