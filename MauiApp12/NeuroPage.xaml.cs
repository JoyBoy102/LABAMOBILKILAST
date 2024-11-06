using System.Net.Http.Headers;
using System.Text.Json;

namespace MauiApp12;

public partial class NeuroPage : ContentPage
{
    public NeuroPage()
    {
        InitializeComponent();
    }
    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync();

            if (result != null)
            {
                var stream = await result.OpenReadAsync();
                SelectedImage.Source = ImageSource.FromStream(() => stream);
            }
            await UploadImage(result);

        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
    private async Task UploadImage(FileResult fileResult)
    {
        using (var client = new HttpClient())
        {
            var url = $"http://{ServerConnectPage.ip}:8000/predict";

            using (var form = new MultipartFormDataContent())
            {
                var stream = await fileResult.OpenReadAsync();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpg"); // Убедитесь, что тип соответствует вашему изображению
                form.Add(fileContent, "file", fileResult.FileName);
                var response = await client.PostAsync(url, form);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Dictionary<string, object> neuroPredict = JsonSerializer.Deserialize<Dictionary<string, object>>(result);
                    string neuroResult = neuroPredict["prediction"].ToString();
                    neuroResult = char.ToUpper(neuroResult[0]) + neuroResult.Substring(1);
                    NeuroPredictionLabel.Text = neuroResult;
                }
                else
                {
                    await DisplayAlert("Ошибка", $"Не удалось отправить изображение: {response.StatusCode}", "OK");
                }
            }
        }
    }
}