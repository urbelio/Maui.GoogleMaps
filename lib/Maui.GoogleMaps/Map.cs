using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Maui.GoogleMaps.Internals;
using Maui.GoogleMaps.Helpers;
using Maui.GoogleMaps.Extensions;
using System.ComponentModel;

namespace Maui.GoogleMaps;

public class Map : View, IMap, IEnumerable<Pin>
{
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(IEnumerable), typeof(IEnumerable), typeof(Map), default(IEnumerable),
        propertyChanged: (b, o, n) => ((Map)b).OnItemsSourcePropertyChanged((IEnumerable)o, (IEnumerable)n));

    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(Map), default(DataTemplate),
        propertyChanged: (b, o, n) => ((Map)b).OnItemTemplatePropertyChanged((DataTemplate)o, (DataTemplate)n));

    public static readonly BindableProperty ItemTemplateSelectorProperty = BindableProperty.Create(nameof(ItemTemplateSelector), typeof(DataTemplateSelector), typeof(Map), default(DataTemplateSelector),
        propertyChanged: (b, o, n) => ((Map)b).OnItemTemplateSelectorPropertyChanged());

    public static readonly BindableProperty MapTypeProperty = BindableProperty.Create(nameof(MapType), typeof(MapType), typeof(Map), default(MapType));

    public static readonly BindableProperty MyLocationEnabledProperty = BindableProperty.Create(nameof(MyLocationEnabled), typeof(bool), typeof(Map), default(bool));
    
    public static readonly BindableProperty SelectedPinProperty = BindableProperty.Create(nameof(SelectedPin), typeof(Pin), typeof(Map), default(Pin), defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty IsTrafficEnabledProperty = BindableProperty.Create(nameof(IsTrafficEnabled), typeof(bool), typeof(Map), false);

    public static readonly BindableProperty IsIndoorEnabledProperty = BindableProperty.Create(nameof(IsIndoorEnabled), typeof(bool), typeof(Map), true);

    public static readonly BindableProperty InitialCameraUpdateProperty = BindableProperty.Create(
        nameof(InitialCameraUpdate),
        typeof(CameraUpdate),
        typeof(Map),
        CameraUpdateFactory.NewPositionZoom(new Position(41.89, 12.49), 10),  // center on Rome by default
        propertyChanged: (bindable, oldValue, newValue) => 
        {
            ((Map)bindable)._useMoveToRegisonAsInitialBounds = false;
        });

    public static readonly BindableProperty PaddingProperty = BindableProperty.Create(nameof(PaddingProperty), typeof(Thickness), typeof(Map), default(Thickness));

    public static readonly BindableProperty CameraPositionProperty = BindableProperty.Create(
        nameof(CameraPosition), typeof(CameraPosition), typeof(Map),
        defaultValueCreator: (bindable) => new CameraPosition(((Map)bindable).InitialCameraUpdate.Position, 10),
        defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty MapStyleProperty = BindableProperty.Create(nameof(MapStyle), typeof(MapStyle), typeof(Map), null);

    readonly ObservableCollection<Pin> _pins = new ObservableCollection<Pin>();
    readonly ObservableCollection<Polyline> _polylines = new ObservableCollection<Polyline>();
    readonly ObservableCollection<Polygon> _polygons = new ObservableCollection<Polygon>();
    readonly ObservableCollection<Circle> _circles = new ObservableCollection<Circle>();
    readonly ObservableCollection<TileLayer> _tileLayers = new ObservableCollection<TileLayer>();
    readonly ObservableCollection<GroundOverlay> _groundOverlays = new ObservableCollection<GroundOverlay>();

    public event EventHandler<PinClickedEventArgs> PinClicked;
    public event EventHandler<SelectedPinChangedEventArgs> SelectedPinChanged;
    public event EventHandler<InfoWindowClickedEventArgs> InfoWindowClicked;
    public event EventHandler<InfoWindowLongClickedEventArgs> InfoWindowLongClicked;

    public event EventHandler<PinDragEventArgs> PinDragStart;
    public event EventHandler<PinDragEventArgs> PinDragEnd;
    public event EventHandler<PinDragEventArgs> PinDragging;

    public event EventHandler<MapClickedEventArgs> MapClicked;
    public event EventHandler<MapLongClickedEventArgs> MapLongClicked;
    public event EventHandler<MyLocationButtonClickedEventArgs> MyLocationButtonClicked;
    public event EventHandler MapReady;

    [Obsolete("Please use Map.CameraIdled instead of this")]
    public event EventHandler<CameraChangedEventArgs> CameraChanged;
    public event EventHandler<CameraMoveStartedEventArgs> CameraMoveStarted;
    public event EventHandler<CameraMovingEventArgs> CameraMoving;
    public event EventHandler<CameraIdledEventArgs> CameraIdled;

    internal Action<MoveToRegionMessage> OnMoveToRegion { get; set; }

    internal Action<CameraUpdateMessage> OnMoveCamera { get; set; }

    internal Action<CameraUpdateMessage> OnAnimateCamera { get; set; }

    internal Action<TakeSnapshotMessage> OnSnapshot{ get; set; }

    internal Func<Point, Position> OnFromScreenLocation { get; set; }
    internal Func<Position, Point> OnToScreenLocation { get; set; }

    MapSpan _visibleRegion;
    MapRegion _region;
    bool _useMoveToRegisonAsInitialBounds = true;

    public Map()
    {
        VerticalOptions = HorizontalOptions = LayoutOptions.Fill;
    }

    public bool IsTrafficEnabled
    {
        get { return (bool)GetValue(IsTrafficEnabledProperty); }
        set { SetValue(IsTrafficEnabledProperty, value); }
    }

    public bool IsIndoorEnabled
    {
        get { return (bool) GetValue(IsIndoorEnabledProperty); }
        set { SetValue(IsIndoorEnabledProperty, value);}
    }

    public bool MyLocationEnabled
    {
        get { return (bool)GetValue(MyLocationEnabledProperty); }
        set { SetValue(MyLocationEnabledProperty, value); }
    }

    public MapType MapType
    {
        get { return (MapType)GetValue(MapTypeProperty); }
        set { SetValue(MapTypeProperty, value); }
    }

    public Pin SelectedPin
    {
        get { return (Pin)GetValue(SelectedPinProperty); }
        set { SetValue(SelectedPinProperty, value); }
    }

    [TypeConverter(typeof(CameraUpdateConverter))]
    public CameraUpdate InitialCameraUpdate
    {
        get { return (CameraUpdate)GetValue(InitialCameraUpdateProperty); }
        set { SetValue(InitialCameraUpdateProperty, value); }
    }

    public CameraPosition CameraPosition
    {
        get { return (CameraPosition)GetValue(CameraPositionProperty); }
        internal set { SetValue(CameraPositionProperty, value); }
    }

    public Thickness Padding
    {
        get { return (Thickness)GetValue(PaddingProperty); }
        set { SetValue(PaddingProperty, value); }
    }

    public MapStyle MapStyle
    {
        get { return (MapStyle)GetValue(MapStyleProperty); }
        set { SetValue(MapStyleProperty, value); }
    }

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public DataTemplateSelector ItemTemplateSelector
    {
        get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
        set { SetValue(ItemTemplateSelectorProperty, value); }
    }

    public IList<Pin> Pins
    {
        get { return _pins; }
    }

    public IList<Polyline> Polylines
    {
        get { return _polylines; }
    }

    public IList<Polygon> Polygons
    {
        get { return _polygons; }
    }

    public IList<Circle> Circles
    {
        get { return _circles; }
    }

    public IList<TileLayer> TileLayers
    {
        get { return _tileLayers; }
    }

    public IList<GroundOverlay> GroundOverlays
    {
        get { return _groundOverlays; }
    }

    [Obsolete("Please use Map.Region instead of this")]
    public MapSpan VisibleRegion
    {
        get { return _visibleRegion; }
        internal set
        {
            if (_visibleRegion == value)
                return;

            ArgumentNullException.ThrowIfNull(value);

            OnPropertyChanging();
            _visibleRegion = value;
            OnPropertyChanged();
        }
    }

    public MapRegion Region
    {
        get { return _region; }
        internal set
        {
            if (_region == value)
            {
                return;
            }

            ArgumentNullException.ThrowIfNull(value);

            OnPropertyChanging();
            _region = value;
            OnPropertyChanged();
        }
    }

    public UiSettings UiSettings { get; } = new UiSettings();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<Pin> GetEnumerator()
    {
        return _pins.GetEnumerator();
    }

    public void MoveToRegion(MapSpan mapSpan, bool animate = true)
    {
        if (mapSpan == null)
            throw new ArgumentNullException(nameof(mapSpan));

        if (_useMoveToRegisonAsInitialBounds)
        {
            InitialCameraUpdate = CameraUpdateFactory.NewBounds(mapSpan.ToBounds(), 0);
            _useMoveToRegisonAsInitialBounds = false;
        }

        SendMoveToRegion(new MoveToRegionMessage(mapSpan, animate));
    }

    public Task<AnimationStatus> MoveCamera(CameraUpdate cameraUpdate)
    {
        var comp = new TaskCompletionSource<AnimationStatus>();

        SendMoveCamera(new CameraUpdateMessage(cameraUpdate, null, new DelegateAnimationCallback(
            () => comp.SetResult(AnimationStatus.Finished), 
            () => comp.SetResult(AnimationStatus.Canceled))));

        return comp.Task;
    }

    public Task<AnimationStatus> AnimateCamera(CameraUpdate cameraUpdate, TimeSpan? duration = null)
    {
        var comp = new TaskCompletionSource<AnimationStatus>();

        SendAnimateCamera(new CameraUpdateMessage(cameraUpdate, duration, new DelegateAnimationCallback(
            () => comp.SetResult(AnimationStatus.Finished),
            () => comp.SetResult(AnimationStatus.Canceled))));

        return comp.Task;
    }

    public Task<Stream> TakeSnapshot()
    {
        var comp = new TaskCompletionSource<Stream>();

        SendTakeSnapshot(new TakeSnapshotMessage(image => comp.SetResult(image)));

        return comp.Task;
    }

    internal void SendSelectedPinChanged(Pin selectedPin)
    {
        SelectedPinChanged?.Invoke(this, new SelectedPinChangedEventArgs(selectedPin));
    }

    void OnItemTemplateSelectorPropertyChanged()
    {
        _pins.Clear();
        CreatePinItems();
    }

    internal bool SendPinClicked(Pin pin)
    {
        var args = new PinClickedEventArgs(pin);
        PinClicked?.Invoke(this, args);
        return args.Handled;
    }

    internal void SendInfoWindowClicked(Pin pin)
    {
        var args = new InfoWindowClickedEventArgs(pin);
        InfoWindowClicked?.Invoke(this, args);
    }

    internal void SendInfoWindowLongClicked(Pin pin)
    {
        var args = new InfoWindowLongClickedEventArgs(pin);
        InfoWindowLongClicked?.Invoke(this, args);
    }

    internal void SendPinDragStart(Pin pin)
    {
        PinDragStart?.Invoke(this, new PinDragEventArgs(pin));
    }

    internal void SendPinDragEnd(Pin pin)
    {
        PinDragEnd?.Invoke(this, new PinDragEventArgs(pin));
    }

    internal void SendPinDragging(Pin pin)
    {
        PinDragging?.Invoke(this, new PinDragEventArgs(pin));
    }

    internal void SendMapClicked(Position point)
    {
        MapClicked?.Invoke(this, new MapClickedEventArgs(point));
    }

    internal void SendMapLongClicked(Position point)
    {
        MapLongClicked?.Invoke(this, new MapLongClickedEventArgs(point));
    }

    internal bool SendMyLocationClicked()
    {
        var args = new MyLocationButtonClickedEventArgs();
        MyLocationButtonClicked?.Invoke(this, args);
        return args.Handled;
    }

    internal void SendCameraChanged(CameraPosition position)
    {
        CameraChanged?.Invoke(this, new CameraChangedEventArgs(position));
    }

    internal void SendCameraMoveStarted(bool isGesture)
    {
        CameraMoveStarted?.Invoke(this, new CameraMoveStartedEventArgs(isGesture));
    }

    internal void SendCameraMoving(CameraPosition position)
    {
        CameraMoving?.Invoke(this, new CameraMovingEventArgs(position));
    }

    internal void SendCameraIdled(CameraPosition position)
    {
        CameraIdled?.Invoke(this, new CameraIdledEventArgs(position));
    }

    internal void SendMapReady()
    {
        MapReady?.Invoke(this, EventArgs.Empty);
    }

    private void SendMoveToRegion(MoveToRegionMessage message)
    {
        OnMoveToRegion?.Invoke(message);
    }

    void SendMoveCamera(CameraUpdateMessage message)
    {
        OnMoveCamera?.Invoke(message);
    }

    void SendAnimateCamera(CameraUpdateMessage message)
    {
        OnAnimateCamera?.Invoke(message);
    }

    void SendTakeSnapshot(TakeSnapshotMessage message)
    {
        OnSnapshot?.Invoke(message);
    }

    void OnItemsSourcePropertyChanged(IEnumerable oldItemsSource, IEnumerable newItemsSource)
    {
        if (oldItemsSource is INotifyCollectionChanged ncc)
        {
            ncc.CollectionChanged -= OnItemsSourceCollectionChanged;
        }

        if (newItemsSource is INotifyCollectionChanged ncc1)
        {
            ncc1.CollectionChanged += OnItemsSourceCollectionChanged;
        }

        _pins.Clear();
        CreatePinItems();
    }

    void OnItemTemplatePropertyChanged(DataTemplate oldItemTemplate, DataTemplate newItemTemplate)
    {
        if (newItemTemplate is DataTemplateSelector)
        {
            throw new NotSupportedException($"You are using an instance of {nameof(DataTemplateSelector)} to set the {nameof(Map)}.{ItemTemplateProperty.PropertyName} property. Use an instance of a {nameof(DataTemplate)} property instead to set an item template.");
        }

        _pins.Clear();
        CreatePinItems();
    }

    void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewStartingIndex == -1)
                {
                    goto case NotifyCollectionChangedAction.Reset;
                }

                foreach (object item in e.NewItems)
                {
                    CreatePin(item);
                }

                break;

            case NotifyCollectionChangedAction.Move:
                if (e.OldStartingIndex == -1 || e.NewStartingIndex == -1)
                {
                    goto case NotifyCollectionChangedAction.Reset;
                }

                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldStartingIndex == -1)
                {
                    goto case NotifyCollectionChangedAction.Reset;
                }

                foreach (object item in e.OldItems)
                {
                    RemovePin(item);
                }

                break;

            case NotifyCollectionChangedAction.Replace:
                if (e.OldStartingIndex == -1)
                {
                    goto case NotifyCollectionChangedAction.Reset;
                }

                foreach (object item in e.OldItems)
                {
                    RemovePin(item);
                }

                foreach (object item in e.NewItems)
                {
                    CreatePin(item);
                }

                break;

            case NotifyCollectionChangedAction.Reset:
                _pins.Clear();
                CreatePinItems();
                break;
        }
    }

    void CreatePinItems()
    {
        if (ItemsSource == null || (ItemTemplate == null && ItemTemplateSelector == null))
        {
            return;
        }

        foreach (object item in ItemsSource)
        {
            CreatePin(item);
        }
    }

    void CreatePin(object newItem)
    {
        DataTemplate itemTemplate = ItemTemplate;
        if (itemTemplate == null)
        {
            itemTemplate = ItemTemplateSelector?.SelectTemplate(newItem, this);
        }

        if (itemTemplate == null)
        {
            return;
        }

        var pin = (Pin)itemTemplate.CreateContent();
        pin.BindingContext = newItem;
        _pins.Add(pin);
    }

    void RemovePin(object itemToRemove)
    {
        Pin pinToRemove = _pins.FirstOrDefault(pin => pin.BindingContext?.Equals(itemToRemove) == true);
        if (pinToRemove != null)
        {
            _pins.Remove(pinToRemove);
        }
    }

    public Position FromScreenLocation(Point point)
    {
        if (OnFromScreenLocation == null)
        {
            throw new NullReferenceException("OnFromScreenLocation");
        }

        return OnFromScreenLocation.Invoke(point);
    }

    public Point ToScreenLocation(Position position)
    {
        if (OnToScreenLocation == null)
        {
            throw new NullReferenceException("ToScreenLocation");
        }

        return OnToScreenLocation.Invoke(position);
    }
}