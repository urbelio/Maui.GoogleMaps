namespace Maui.GoogleMaps
{
    public sealed class ClusterClickedEventArgs : EventArgs
    {
        public bool Handled { get; set; } = false;
        public List<string> Ids { get; }
        public ClusterPin Cluster { get; }

        internal ClusterClickedEventArgs(List<string> ids, ClusterPin cluster)
        {
            Ids = ids;
            Cluster = cluster;
        }
    }
}
