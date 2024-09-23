namespace Maui.GoogleMaps
{
	public class RegionChangedEventArgs : EventArgs
	{
		public MapRegion Region { get; set; }
		internal RegionChangedEventArgs( MapRegion region)
		{
			Region = region;
		}
	}
}

