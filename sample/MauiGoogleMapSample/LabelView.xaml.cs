using Maui.GoogleMaps.Views;

namespace MauiGoogleMapSample;

public partial class LabelView : ContentView, IClusterView
{
    public LabelView()
    {
        InitializeComponent();
    }

    //public LabelView(string text)
    //{
    //	InitializeComponent();
    //	LabelContainer.MaxLines = text.Length > 15 ? 2 : 1;
    //	Container.HeightRequest = text.Length > 15 ? 150 : 100;
    //       LabelContainer.Text = text;
    //   }

    public void ChangeClusterText(string text)
    {
        LabelContainer.MaxLines = text.Length > 15 ? 2 : 1;
        Container.HeightRequest = text.Length > 15 ? 150 : 100;
        LabelContainer.Text = text;
    }

    public void ChangeSize(int size)
    {
        Container.HeightRequest = size;
    }

    public IClusterView Instance()
    {
        return new LabelView();
    }
}

