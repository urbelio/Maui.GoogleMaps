namespace Maui.GoogleMaps
{
    public sealed class SelectedClusterChangedEventArgs : EventArgs
    {
        public ClusterPin SelectedPin
        {
            get;
            private set;
        }

        internal SelectedClusterChangedEventArgs(ClusterPin selectedPin)
        {
            this.SelectedPin = selectedPin;
        }
    }
}

