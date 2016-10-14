using Acr.UserDialogs;
using AsyncOAuth;
using Hatena;
using Hatenatter.Modles;
//using Java.Lang;
using Newtonsoft.Json;
using PCLCrypto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
//using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Forms;

namespace Hatenatter
{
    
    public partial class Page1 : ContentPage
    {
        Random m_random = new Random();
        public UserViewModel m_myInfo { get; set; }
        public ListViewModel m_list { get; set; }
        HatenaClient m_client = null;

        public Page1()
        {
            InitializeComponent();

            // 
            m_myInfo = new UserViewModel { Id = "unknown", DisplayName = "unknown", Image = "login.png" };
            MyUserLayout.BindingContext = m_myInfo;

            // Make data list
            m_list = new ListViewModel();
            m_list.Add(
                new ItemInfo
                {
                    Image = "https://avatars0.githubusercontent.com/u/20608487?v=3&s=200",
                    Name = "John",
                    State = "Hello"
                }
            );
            m_list.Add(
                new ItemInfo
                {
                    Image = "https://avatars0.githubusercontent.com/u/20608487?v=3&s=200",
                    Name = "Kei",
                    State = "Hello"
                }
            );
            m_list.Add(
                new ItemInfo
                {
                    Image = "https://avatars0.githubusercontent.com/u/20608487?v=3&s=200",
                    Name = "Tama",
                    State = "Hello"
                }
            );

            // Bind
            //this.BindingContext = m_list;
            MyListFrame.BindingContext = m_list;
            //MyList.ItemsSource = m_list.MyListData;

            // 右上ユーザ画像タップ
            var profileTapRecognizer = new TapGestureRecognizer
            {
                Command = new Command(() => {
                    OnUserIconClicked();
                }),
                NumberOfTapsRequired = 1
            };
            UserIcon.GestureRecognizers.Add(profileTapRecognizer);
        }

        void OnUserIconClicked()
        {
            Task.Run(async () =>
            {
                await AuthButton_Clicked(null, null);
            });
        }

        async void RefreshButton_Clicked(object sender, EventArgs e)
        {
            // タイムライン更新
            RefreshButton.IsEnabled = false;
            await Task.Delay(1500);
            RefreshButton.IsEnabled = true;
        }

        bool m_loginProceeding = false;
        async Task AuthButton_Clicked(object sender, EventArgs e)
        {
            if (m_loginProceeding) return;
            m_loginProceeding = true;

            // ログイン
            string result = await StartAuth();

            // JSONパース
            // {"profile_image_url":"http://cdn1......gif?111111", "url_name":"kobake", "display_name": "kobake"}
            bool jsonOk = false;
            try
            {
                Dictionary<string, string> mymap = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                m_myInfo.Id = mymap["url_name"];
                m_myInfo.DisplayName = mymap["display_name"];
                m_myInfo.Image = mymap["profile_image_url"];
                jsonOk = true;
            }
            catch(System.Exception ex)
            {
            }
            Debug.WriteLine("=================THREAD_ID = " + Java.Lang.Thread.CurrentThread().Id);

            // 結果表示
            if (!jsonOk)
            {
                await DisplayAlert("Title", "result = " + result, "OK");
            }

            m_loginProceeding = false;
        }

        

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

            var sBuilder = new System.Text.StringBuilder();
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

        private async Task<string> StartAuth()
        {
            // initialize computehash function
            OAuthUtility.ComputeHash = (key, buffer) =>
            {
                return hash_hmacSha1(buffer, key);
            };

            try
            {
                var accessToken = await HatenaLogin.Authorize();
                if (accessToken == null) throw new System.Exception("login error");

                m_client = new HatenaClient(accessToken);

                var my = await m_client.GetMy();
                Debug.WriteLine("my = " + my);

                

                return my;
            }
            catch (Java.Lang.Exception ex)
            {
                return "error: " + ex.Message;
            }
            catch (System.Exception ex)
            {
                return "error: " + ex.Message;
            }
        }
    }
}
