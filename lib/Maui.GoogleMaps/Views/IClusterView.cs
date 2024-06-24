namespace Maui.GoogleMaps.Views
{
    public interface IClusterView
    {
        public void ChangeSize(int size);
        public void ChangeClusterText(string text);
        public IClusterView Instance();
    }
}

