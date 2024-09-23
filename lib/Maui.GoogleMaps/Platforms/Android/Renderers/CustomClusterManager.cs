using System.Collections;
using System.Diagnostics;
using Android.Content;
using Android.Gms.Maps;
using Com.Google.Maps.Android.Clustering;
using Com.Google.Maps.Android.Collections;

namespace Maui.GoogleMaps.Platforms.Android.Renderers
{
    public class CustomClusterManager : ClusterManager
    {
        public CustomClusterManager(Context context, GoogleMap map, MarkerManager markerManager) : base(context, map, markerManager)
        {
        }

        public override bool AddItems(ICollection items)
        {
            return base.AddItems(items);
        }

        public override bool AddItem(Java.Lang.Object myItem)
        {
            return base.AddItem(myItem);
        }
        public override void ClearItems()
        {
            base.ClearItems();
        }

        public override bool RemoveItem(Java.Lang.Object item)
        {
            try
            {
                if (((IClusterItem)item).Position != null)
                {
                    return base.RemoveItem(item);
                }
                return false;
            }
            catch(Exception exc)
            {
                Debug.WriteLine($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!{((IClusterItem)item).Snippet} - {exc.Message}!!!!!!!!!!!!!!!!!!!!!!!!");
                return false;
            }
        }
    }
}

