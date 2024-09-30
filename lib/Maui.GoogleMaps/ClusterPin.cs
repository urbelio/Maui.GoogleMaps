namespace Maui.GoogleMaps
{
    public class ClusterPin : BindableObject, IPointForClustering
    {

        public static readonly BindableProperty PositionProperty = BindableProperty.Create(nameof(Position), typeof(Position), typeof(ClusterPin), default(Position));

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(ClusterPin), default(string));

        public static readonly BindableProperty SnippetProperty = BindableProperty.Create(nameof(Snippet), typeof(string), typeof(ClusterPin), default(string));

        //public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(BitmapDescriptor), typeof(ClusterPin), default(BitmapDescriptor));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Snippet
        {
            get { return (string)GetValue(SnippetProperty); }
            set { SetValue(SnippetProperty, value); }
        }

        public Position Position
        {
            get { return (Position)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        //public BitmapDescriptor Icon
        //{
        //    get { return (BitmapDescriptor)GetValue(IconProperty); }
        //    set { SetValue(IconProperty, value); }
        //}

        public object Tag { get; set; }

        public object NativeObject { get; internal set; }

        [Obsolete("Please use Map.PinClicked instead of this")]
        public event EventHandler Clicked;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((ClusterPin)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Title?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ Position.GetHashCode();
                hashCode = (hashCode * 397) ^ (Snippet?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(ClusterPin left, ClusterPin right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ClusterPin left, ClusterPin right)
        {
            return !Equals(left, right);
        }

        internal bool SendTap()
        {
            EventHandler handler = Clicked;
            if (handler == null)
                return false;

            handler(this, EventArgs.Empty);
            return true;
        }

        bool Equals(ClusterPin other)
        {
            return string.Equals(Title, other.Title) && Equals(Position, other.Position) && string.Equals(Snippet, other.Snippet);
        }

        public Position GetPosition()
        {
            return Position;
        }

        //public BitmapDescriptor GetIcon()
        //{
        //    return Icon;
        //}

        public string GetTitle()
        {
            return Title;
        }

        public string GetSnippet()
        {
            return Snippet;
        }
    }
}
