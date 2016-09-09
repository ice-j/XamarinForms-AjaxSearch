using RestSharp.Portable.HttpClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Windows.Input;

namespace XamarinForms_AjaxSearch
{
    public class SearchList : ContentView
    {
        protected ListView ResultsList { get; set; } = new ListView();
        public ObservableCollection<SearchListSelectItem> Results { get; set; } = new ObservableCollection<SearchListSelectItem>();
        HttpClient _httpClient = new HttpClient();
        protected Entry Search { get; set; } = new Entry();

        ICommand _itemSelectedCommand;
        public ICommand ItemSelectedCommand
        {
            get
            {
                return _itemSelectedCommand ?? new Command(ItemSelected);
            }
            set
            {
                _itemSelectedCommand = value;
            }
        }

        private void ItemSelected(object obj)
        {
            
        }

        public Uri UriSource
        {
            get
            {
                return (Uri)GetValue(UriSourceProperty);
            }
            set
            {
                SetValue(UriSourceProperty, value);
                _httpClient.BaseAddress = UriSource;
            }
        }
        public BindableProperty UriSourceProperty = BindableProperty.Create(nameof(UriSource), typeof(Uri), typeof(SearchList), new Uri("http://hummingbird.me/"));

        public SearchList()
        {
            Search.Placeholder = "Name...";
            Search.TextChanged += Search_TextChanged;
            BindingContext = this;
            ResultsList.SetBinding<SearchList>(ListView.ItemsSourceProperty, x => x.Results);
            ResultsList.ItemSelected += (sender, args) => ItemSelectedCommand.Execute(args.SelectedItem);
            _httpClient.BaseAddress = UriSource;
            ResultsList.ItemTemplate = new DataTemplate(() =>
            {
                var viewCell = new ViewCell();
                var name = new Label { Margin = new Thickness(15, 0), LineBreakMode = LineBreakMode.TailTruncation };
                name.SetBinding<SearchListSelectItem>(Label.TextProperty, x => x.Name);
                viewCell.View = name;
                return viewCell;
            });
            Content = new StackLayout
            {
                Children =
                {
                    Search,
                    ResultsList
                }
            };
        }

        private async void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTextValue) || e.NewTextValue.Length < 3)
            {
                Results.Clear();
                return;
            }
            ResultsList.IsRefreshing = true;
            var response = await _httpClient.GetAsync("api/v1/search/anime?query="+e.NewTextValue, HttpCompletionOption.ResponseContentRead);
            if (response.IsSuccessStatusCode)
            {
                Results.Clear();
                var list = JsonConvert.DeserializeObject<List<SelectItemTemplate>>(await response.Content.ReadAsStringAsync());
                if (list == null || list.Count < 1)
                    return;

                foreach (var item in list)
                    Results.Add(new SearchListSelectItem
                    {
                        Id = item.Id,
                        Name = item.Title
                    });
            }

            ResultsList.IsRefreshing = false;
        }

        private class SelectItemTemplate
        {
            public string Id { get; set; }
            public string Title { get; set; }
        }
    }


    public class SearchListSelectItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
