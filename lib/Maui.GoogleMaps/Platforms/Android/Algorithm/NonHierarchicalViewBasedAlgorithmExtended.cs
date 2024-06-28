using Android.Runtime;
using Com.Google.Maps.Android.Clustering.Algo;

namespace Maui.GoogleMaps.Platforms.Android.Algorithm
{
    public class NonHierarchicalViewBasedAlgorithmExtended : NonHierarchicalViewBasedAlgorithm
    {
        public NonHierarchicalViewBasedAlgorithmExtended(int widthDp, int heightDp) : base(widthDp, heightDp) { }

        protected NonHierarchicalViewBasedAlgorithmExtended(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override bool ShouldReclusterOnMapMovement()
        {
            return true;
        }
    }
}

