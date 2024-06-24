using Maui.GoogleMaps.Views;

namespace MauiGoogleMapSample;

public partial class ClusterView : ContentView, IClusterView
{
    public static readonly BindableProperty LabelTextProperty = BindableProperty.Create(nameof(LabelText), typeof(string), typeof(ClusterView), string.Empty);

    public string LabelText
    {
        get { return (string)GetValue(LabelTextProperty); }
        set { SetValue(LabelTextProperty, value); }
    }

    public ClusterView()
    {
        InitializeComponent();
    }

    public void ChangeSize(int size)
    {
        BorderContainer.HeightRequest = size;
        BorderContainer.WidthRequest = size;
    }

    public void ChangeClusterText(string text)
    {
        LabelText = text;
    }

    public IClusterView Instance()
    {
        return new ClusterView();
    }
}
