namespace Maui.GoogleMaps
{
    public interface IPointForClustering
    {
        public Position GetPosition();
        //public BitmapDescriptor GetIcon();
        public string GetTitle();
        public string GetSnippet();
    }
}

