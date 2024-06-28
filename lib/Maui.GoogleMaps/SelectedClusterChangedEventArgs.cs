namespace Maui.GoogleMaps
{
    public sealed class SelectedClusterChangedEventArgs : EventArgs
    {
        public ClusterPin SelectedCluster
        {
            get;
            private set;
        }

        internal SelectedClusterChangedEventArgs(ClusterPin selectedPin)
        {
            this.SelectedCluster = selectedPin;
        }
    }
}

