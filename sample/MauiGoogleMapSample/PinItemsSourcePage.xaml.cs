// via https://github.com/andreinitescu/Xamarin.Forms/blob/69230b7564cbd2eb26947a8c75de159a373f7a63/Xamarin.Forms.Controls/GalleryPages/MapWithItemsSourceGallery.xaml
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Maui.GoogleMaps;

namespace MauiGoogleMapSample
{
    public partial class PinItemsSourcePage : ContentPage
    {
        public static readonly Position startPosition = new Position(41.89, 12.49);

        public PinItemsSourcePage()
        {
            InitializeComponent();
            
            BindingContext = new ViewModel();
            _map.InitialCameraUpdate = CameraUpdateFactory.NewPositionZoom(startPosition, 5);
        }
    }
    class ViewModel
    {
        int _pinCreatedCount = 0;

        readonly ObservableCollection<ClusterPin> _places;

        public IEnumerable Places => _places;

        public ICommand AddPlaceCommand { get; }
        public ICommand RemovePlaceCommand { get; }
        public ICommand ClearPlacesCommand { get; }
        public ICommand UpdatePlacesCommand { get; }
        public ICommand ReplacePlaceCommand { get; }

        public ViewModel()
        {
            Position startPos = new Position(41.9027835, 12.4963655);

            double fromLong = -0.5;
            double toLong = 0.5;
            double increaseBy = 0.025;
            double fromLat = -0.5;
            double toLat = 0.5;
            double increaseByLat = 0.025;

            int count = 0;
            var list = new ObservableCollection<ClusterPin>();
            for (double lon = fromLong; lon <= toLong; lon += increaseBy)
            {
                for (double lat = fromLat; lat <= toLat; lat += increaseByLat)
                {
                    var pin = new ClusterPin {
                        Position = new Position(startPos.Latitude + lat, startPos.Longitude + lon),
                        Title = count.ToString(),
                        Snippet = count.ToString()
                    };

                    list.Add(pin);

                    ++count;
                }
            }
            _places = list;
            //_places = new ObservableCollection<Place>() {
            //        new Place("New York, USA", "The City That Never Sleeps", new Position(40.67, -73.94), 1),
            //        new Place("Los Angeles, USA", "City of Angels", new Position(34.11, -118.41), 2),
            //        new Place("San Francisco, USA", "Bay City ", new Position(37.77, -122.45), 3)
            //    };

            AddPlaceCommand = new Command(AddPlace);
            RemovePlaceCommand = new Command(RemovePlace);
            ClearPlacesCommand = new Command(() => _places.Clear());
            UpdatePlacesCommand = new Command(UpdatePlaces);
            ReplacePlaceCommand = new Command(ReplacePlace);
        }

        void AddPlace()
        {
            _places.Add(NewPlace());
        }

        void RemovePlace()
        {
            if (_places.Any())
            {
                _places.Remove(_places.First());
            }
        }

        void UpdatePlaces()
        {
            if (!_places.Any())
            {
                return;
            }

            double lastLatitude = _places.Last().Position.Latitude;

            foreach (Place place in Places)
            {
                place.Position = new Position(lastLatitude, place.Position.Longitude);
                place.IconNumber = 3;
            }
        }

        void ReplacePlace()
        {
            if (!_places.Any())
            {
                return;
            }

            _places[_places.Count - 1] = NewPlace();
        }

        static class RandomPosition
        {
            static Random Random = new Random(Environment.TickCount);

            public static Position Next()
            {
                return new Position(
                    latitude: Random.NextDouble() * 180 - 90,
                    longitude: Random.NextDouble() * 360 - 180);
            }

            public static Position Next(Position position, double latitudeRange, double longitudeRange)
            {
                return new Position(
                    latitude: position.Latitude + (Random.NextDouble() * 2 - 1) * latitudeRange,
                    longitude: position.Longitude + (Random.NextDouble() * 2 - 1) * longitudeRange);
            }
        }

        ClusterPin NewPlace()
        {
            var rand = new Random(Environment.TickCount);
            ++_pinCreatedCount;

            return new ClusterPin {
                Title = $"Pin {_pinCreatedCount}",
                Snippet = $"Desc {_pinCreatedCount}",
                Position = RandomPosition.Next(PinItemsSourcePage.startPosition, 8, 19)};
        }
    }

    class Place : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Address { get; }

        public string Description { get; }

        Position _position;
        public Position Position
        {
            get => _position;
            set
            {
                if (!_position.Equals(value))
                {
                    _position = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
                }
            }
        }

        int _iconNumber;
        public int IconNumber 
        { 
            get => _iconNumber;
            set
            {
                if (_iconNumber != value)
                {
                    _iconNumber = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconNumber)));
                }
            }
        }

        public Place(string address, string description, Position position, int iconNumber)
        {
            Address = address;
            Description = description;
            Position = position;
            IconNumber = iconNumber;
        }
    }

    public class IconConverter : IValueConverter
    {
        private readonly Color[] _colors = new Color[] 
        {
            Colors.Yellow,
            Colors.Gray,
            Colors.Green,
            Colors.Azure,
            Colors.Pink
        };

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var iconNo = value as int?;
            if (iconNo != null)
            {
                return BitmapDescriptorFactory.DefaultMarker(_colors[iconNo.Value]);
            }
            else
            {
                return null;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class MapItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return DataTemplate;
        }
    }
}
