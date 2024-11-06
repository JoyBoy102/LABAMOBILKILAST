using System.Text.Json;

namespace MauiApp12
{
    public partial class MainPage : ContentPage
    {
        List<string> urls;
        List<RadioButton> savedRadioButtons;
        List<RadioButton> RadioButtons;
        private CancellationTokenSource _cancellationTokenSource;
        public MainPage()
        {
            InitializeComponent();
            ArrangeButtons();
            savedRadioButtons = new List<RadioButton>();
            RadioButtons = new List<RadioButton>();
            LikeButton.Clicked += LikeButton_Clicked;
        }

        private static readonly HttpClientHandler handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };

        private static readonly HttpClient client = new HttpClient(handler);

        private async Task<List<string>> GetUrlsAsync()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"http://{ServerConnectPage.ip}:8000/get_urls");
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                List<string> newList = JsonSerializer.Deserialize<List<string>>(json);
                newList.Reverse();
                return newList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting URLs: {ex.Message}");
                return new List<string>();
            }
        }

        private async void ArrangeButtons()
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            RadioButtons = new List<RadioButton>();
            urls = await GetUrlsAsync();
            HttpResponseMessage response_all_dicts = await client.GetAsync($"http://{ServerConnectPage.ip}:8000/get_all_dicts");
            string all_files = await response_all_dicts.Content.ReadAsStringAsync();
            if (all_files != "{}")
            {
                Dictionary<string, Dictionary<string, string>> all_diseases = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(all_files);
                foreach (var disease in all_diseases)
                {
                    AddButtonsToList(disease.Value);
                }
                AddButtonsToStackLayout(RadioButtons);
            }
            else
            {
                foreach (string url in urls)
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync($"http://{ServerConnectPage.ip}:8000/get_dict?url={url}");
                        response.EnsureSuccessStatusCode();
                        string json = await response.Content.ReadAsStringAsync();
                        Dictionary<string, string> diseases = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                        AddButtonsToList(diseases);
                    }
                    catch
                    {
                        continue;
                    }
                }
                AddButtonsToStackLayout(RadioButtons);
            }
        }

        private void RadioButton_CheckedChanged(object? sender, CheckedChangedEventArgs e)
        {

            RadioButton current_radioButton = (RadioButton)sender;
            if (current_radioButton.IsChecked)
            {
                Dictionary<string, string> diseaseInfo = (Dictionary<string, string>)current_radioButton.BindingContext;
                if (diseaseInfo != null)
                {
                    foreach (var item in diseaseInfo)
                    {
                        Label title = new Label
                        {
                            Text = item.Key,
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            Margin = 5
                        };
                        Label info = new Label
                        {
                            Text = ConvertToNormalForm(item.Value),
                            FontSize = 12,
                            Margin = 5
                        };
                        if (title.Text != "Почему «СМ-Клиника»?" && title.Text != "Получить консультацию")
                        {
                            StackLayout2.Children.Add(title);
                            StackLayout2.Children.Add(info);
                        }
                    }
                }
            }
            else
            {
                StackLayout2.Children.Clear();
            }

        }

        private void AddButtonsToList(Dictionary<string, string> diseases)
        {
            if (diseases != null)
            {
                string disease_name = diseases.Values.First();

                RadioButton newButton = new RadioButton
                {
                    Content = disease_name,
                    BindingContext = diseases
                };
                newButton.CheckedChanged += RadioButton_CheckedChanged;
                RadioButtons.Add(newButton);

            }
        }

        private string ConvertToNormalForm(string string_list)
        {
            string new_str = "";
            if (string_list[0] == '[' && string_list[string_list.Length - 1] == ']')
            {
                List<string> list = string_list.Split("---").ToList();
                list[0] = list[0][1..(list[0].Length - 1)];
                list[list.Count() - 1] = list[list.Count() - 1][0..(list[list.Count() - 1].Length - 2)];
                foreach (var item in list)
                {
                    new_str += "●" + item + '\n' + '\n';
                }
            }
            else
            {
                return string_list;
            }
            return new_str;
        }


        private RadioButton GetCurrentCheckedRadioButton()
        {
            foreach (var child in StackLayout1.Children)
            {
                if (child is RadioButton)
                {
                    RadioButton rbutton = (RadioButton)child;
                    if (rbutton.IsChecked)
                        return rbutton;
                }
            }
            return null;
        }

        private async void LikeButton_Clicked(object sender, EventArgs e)
        {
            RadioButton currentCheckedRadioButton = GetCurrentCheckedRadioButton();
            if (currentCheckedRadioButton != null)
                savedRadioButtons.Add(currentCheckedRadioButton);
            SaveIcon.Opacity = 0;
            SaveIcon.IsVisible = true;

            // Плавное появление
            await SaveIcon.FadeTo(1, 500);
            await Task.Delay(1000);
            await SaveIcon.FadeTo(0, 500);
        }

        private void SavedDiseasesButton_Clicked(object sender, EventArgs e)
        {
            StackLayout1.Children.Clear();
            StackLayout2.Children.Clear();

            foreach (RadioButton rbutton in savedRadioButtons)
            {
                if (rbutton.Parent is Layout parentLayout)
                {
                    parentLayout.Children.Remove(rbutton);
                }
                rbutton.IsChecked = false;
                StackLayout1.Children.Add(rbutton);
            }
        }

        private void ReturnButton_Clicked(object sender, EventArgs e)
        {
            StackLayout1.Children.Clear();
            StackLayout2.Children.Clear();
            foreach (RadioButton rbutton in RadioButtons)
            {
                if (rbutton.Parent is Layout parentLayout)
                {
                    parentLayout.Children.Remove(rbutton);
                }


                StackLayout1.Children.Add(rbutton);
            }
        }

        private void AddButtonsToStackLayout(List<RadioButton> buttons)
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            foreach (var button in buttons)
            {
                StackLayout1.Children.Add(button);
            }
        }

        private async void NeuroButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NeuroPage());
        }

        private async void MapButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MapPage());
        }

        private List<RadioButton> FindInButtonsList()
        {
            if (string.IsNullOrWhiteSpace(Search.Text))
            {
                return new List<RadioButton>(RadioButtons);
            }
            return RadioButtons
                .Where(button => button.Content.ToString().Contains(Search.Text, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }


        private async void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Отменяем предыдущий запрос, если он все еще выполняется
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                // Задержка перед выполнением поиска
                await Task.Delay(300, _cancellationTokenSource.Token); // Задержка в 300 мс
                List<RadioButton> NewRadioButtons = FindInButtonsList();
                StackLayout1.Children.Clear();
                AddButtonsToStackLayout(NewRadioButtons);
            }
            catch (TaskCanceledException)
            {
                // Игнорируем, если задача была отменена
            }
        }
    }

}
