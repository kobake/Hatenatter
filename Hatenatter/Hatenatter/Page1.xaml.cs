using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Hatenatter
{
    public partial class Page1 : ContentPage
    {
        Random m_random = new Random();
        

        public Page1()
        {
            InitializeComponent();

            // Make data list
            List<ItemInfo> list = new List<ItemInfo>();
            list.Add(
                new ItemInfo
                {
                    Image = "https://avatars0.githubusercontent.com/u/20608487?v=3&s=200",
                    Name = "John",
                    State = "Hello"
                }
            );
            list.Add(
                new ItemInfo
                {
                    Image = "https://avatars0.githubusercontent.com/u/20608487?v=3&s=200",
                    Name = "Kei",
                    State = "Hello"
                }
            );
            list.Add(
                new ItemInfo
                {
                    Image = "https://avatars0.githubusercontent.com/u/20608487?v=3&s=200",
                    Name = "Tama",
                    State = "Hello"
                }
            );

            // Bind
            this.BindingContext = list;
        }

        private async void AuthButton_Clicked(object sender, EventArgs e)
        {
            // DisplayAlert("Title", "Message", "OK");
            // OAuth
            int result = await StartAuth();
            await DisplayAlert("Title", "result=" + result, "OK");
        }

        private async Task<int> StartAuth()
        {
            
            var httpClient = new HttpClient(); // Xamarin supports HttpClient!

            Task<string> contentsTask = httpClient.GetStringAsync("http://xamarin.com"); // async method!

            // await! control returns to the caller and the task continues to run on another thread
            string contents = await contentsTask;

            TestLabel.Text += "DownloadHomepage method continues after async call. . . . .\n";

            // After contentTask completes, you can calculate the length of the string.
            int exampleInt = contents.Length;

            TestLabel.Text += "Downloaded the html and found out the length.\n\n\n";

            TestLabel.Text += contents; // just dump the entire HTML

            return exampleInt; // Task<TResult> returns an object of type TResult, in this case int
        }
    }
}
