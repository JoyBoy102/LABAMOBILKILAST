namespace MauiApp12;

public partial class MapPage : ContentPage
{
	public MapPage()
	{
		InitializeComponent();
        webView.Source = $"http://{ServerConnectPage.ip}/index.html";
    }
}