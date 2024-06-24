namespace Maui.GoogleMaps
{
    public sealed class ClusterClickedEventArgs : EventArgs
    {
        public bool Handled { get; set; } = false;
        public ClusterPin Cluster { get; }

        internal ClusterClickedEventArgs(ClusterPin cluster)
        {
            this.Cluster = cluster;
        }
    }
}
