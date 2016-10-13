using AsyncOAuth;
using HatenatterConsole;
using PCLCrypto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
//using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Auth;
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
            await DisplayAlert("Title", "result", "OK");
            //System.Diagnostics.Debug.WriteLine("========== AuthButton_Clicked");
        }

        // set your token
        const string consumerKey = "OjrD3wav+EZbSw==";
        const string consumerSecret = "yKV005kzISrG63/vk1l5mApZi8I=";

        /*
        class HMACSHA1 : IDisposable
        {
            public HMACSHA1(byte[] b) { }
            public byte[] ComputeHash(byte[] b) { return null; }
            public void Dispose()
            {
            }
        }
        */

        // http://stackoverflow.com/questions/36700530/sha1-keyed-hash-with-hmac-in-xamarin-forms-pcl-c-equivalent-to-hash-hmac-in-ph
        // .NET 4.6 以降であれば HMACSHA1 が使えるが、Xamarin だと 4.5 になってしまうので自前でやる
        public static string hash_hmacSha1(string data, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            var algorithm = WinRTCrypto.MacAlgorithmProvider.OpenAlgorithm(MacAlgorithm.HmacSha1);
            CryptographicHash hasher = algorithm.CreateHash(keyBytes);
            hasher.Append(dataBytes);
            byte[] mac = hasher.GetValueAndReset();

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < mac.Length; i++)
            {
                sBuilder.Append(mac[i].ToString("X2"));
            }
            return sBuilder.ToString().ToLower();
        }
        public static byte[] hash_hmacSha1(byte[] dataBytes, byte[] keyBytes)
        {
            var algorithm = WinRTCrypto.MacAlgorithmProvider.OpenAlgorithm(MacAlgorithm.HmacSha1);
            CryptographicHash hasher = algorithm.CreateHash(keyBytes);
            hasher.Append(dataBytes);
            byte[] mac = hasher.GetValueAndReset();
            return mac;
        }

        private async Task<int> StartAuth()
        {
            if (true)
            {
                // initialize computehash function
                OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    return hash_hmacSha1(buffer, key);
                    /*
                    using (var hmac = new HMACSHA1(key))
                    {
                        return hmac.ComputeHash(buffer);
                    }
                    */
                };

                // sample, twitter access flow
                var accessToken = await HatenaClient.AuthorizeSample(consumerKey, consumerSecret);

                var client = new HatenaClient(consumerKey, consumerSecret, accessToken);

                //var tl = await client.GetTimeline(10, 1);
                //Console.WriteLine(tl);
                var my = await client.GetMy();
                Debug.WriteLine("my = " + my);
                return 0;
            }
            if (false)
            {
                // https://components.xamarin.com/view/xamarin.auth
                var auth = new OAuth2Authenticator(
                    clientId: "OjrD3wav+EZbSw==",
                    clientSecret: "yKV005kzISrG63/vk1l5mApZi8I=",
                    scope: "read_public",
                    authorizeUrl: new Uri("https://www.hatena.ne.jp/touch/oauth/authorize"),
                    redirectUrl: new Uri("http://localhost"), //?
                    accessTokenUrl: new Uri("https://www.hatena.com/oauth/token")
                );

                auth.Completed += (sender, eventArgs) =>
                {
                    //DismissViewController(true, null);
                    if (eventArgs.IsAuthenticated)
                    {
                        // Use eventArgs.Account to do wonderful things
                    }
                };

                //var intent = auth.get

                //PresentViewController(auth.GetUI(), true, null);
                return 0;
            }

            // RequestToken取得
            if (false)
            {
                var client = new HttpClient();
                string url = " https://www.hatena.com/oauth/initiate ";

                // 共通ヘッダ設定
                // http://stackoverflow.com/questions/14627399/setting-authorization-header-of-httpclient
                // http://stackoverflow.com/questions/19039450/adding-authorization-to-the-headers
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                //var headers = new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                //headers.
                //client.DefaultRequestHeaders.Authorization = headers;
                //client.DefaultRequestHeaders.Add("Authorization", "");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("");

                // 送信データ
                var content = new StringContent("scope=read_public");
                
                await client.PostAsync(url, content);
            }

            // 通信サンプル
            if (false)
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
}
