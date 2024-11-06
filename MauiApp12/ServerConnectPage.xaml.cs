namespace MauiApp12;

public partial class ServerConnectPage : ContentPage
{
    public static string? ip;
    public ServerConnectPage()
    {
        InitializeComponent();
    }

    private async void OnCheckIP_Clicked(object sender, EventArgs e)
    {
        ip = IpEntry.Text.ToLower();
        await Navigation.PushAsync(new MainPage());
    }
}