using System.ComponentModel;

namespace Maui.GoogleMaps.Logics;

internal abstract class DefaultClusterLogic<TNative, TNativeMap> : DefaultLogic<ClusterPin, TNative, TNativeMap>
    where TNative : class
    where TNativeMap : class
{
    protected override void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.OnItemPropertyChanged(sender, e);
        var outerItem = sender as ClusterPin;

        if (sender is not ClusterPin { NativeObject: TNative nativeItem })
        {
            return;
        }

        if (e.PropertyName == ClusterPin.TitleProperty.PropertyName) OnUpdateTitle(outerItem, nativeItem);
        else if (e.PropertyName == ClusterPin.SnippetProperty.PropertyName) OnUpdateSnippet(outerItem, nativeItem);
        else if (e.PropertyName == Pin.PositionProperty.PropertyName) OnUpdatePosition(outerItem, nativeItem);
    }

    protected override void CheckCanCreateNativeItem(ClusterPin outerItem)
    {
        if (outerItem.Title == null)
        {
            throw new ArgumentException("Pin must have a Label to be added to a map");
        }
    }

    protected abstract void OnUpdateTitle(ClusterPin outerItem, TNative nativeItem);

    protected abstract void OnUpdateSnippet(ClusterPin outerItem, TNative nativeItem);

    protected abstract void OnUpdatePosition(ClusterPin outerItem, TNative nativeItem);
}