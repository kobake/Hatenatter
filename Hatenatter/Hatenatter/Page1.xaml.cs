using Acr.UserDialogs;
using AsyncOAuth;
using Hatena;
using Hatenatter.Models;
using Hatenatter.Modles;
using Newtonsoft.Json;
using PCLCrypto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
//using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Forms;
using System.IO;

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
            m_myInfo = new UserViewModel { Id = "", DisplayName = "unknown", Image = "login.png" };
            MyUserLayout.BindingContext = m_myInfo;

            // Make data list
            m_list = new ListViewModel();
            m_list.Add(
                new TimelineItem
                {
                    ArticleName = "記事",
                    ArticleUrl = "http://",
                    Comment = new HatenaComment
                    {
                        UserId = "",
                        Comment = "ログインしてください",
                        Date = "",
                    }
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

            // リストビュータップ
            MyList.ItemTapped += OnItemTapped;
        }

        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            Debug.WriteLine("OnItemTapped: " + e.Item.ToString());

            TimelineItem item = e.Item as TimelineItem;
            if (item != null)
            {
                if (!string.IsNullOrEmpty(item.HatenaUrl))
                {
                    Debug.WriteLine("OpenByTapItem: " + item.HatenaUrl);
                    Device.OpenUri(new Uri(item.HatenaUrl));
                }
            }
        }

        void OnUserIconClicked()
        {
            Task.Run(async () =>
            {
                await AuthButton_Clicked(null, null);
            });
        }

        // タイムライン更新
        async void RefreshButton_Clicked(object sender, EventArgs e)
        {
            // ログインしてない場合は何もしない
            if (string.IsNullOrEmpty(m_myInfo.Id))
            {
                Debug.WriteLine("Error: required to login");
                return;
            }

            // 処理中はボタン無効
            RefreshButton.IsEnabled = false;

            // ローディング表示
            var config = new ProgressDialogConfig()
                .SetTitle("Load Timeline ...")
                .SetIsDeterministic(false)
                .SetMaskType(MaskType.Black);
            string error = "";
            using (UserDialogs.Instance.Progress(config))
            {
                await Task.Delay(300);
                try
                {
                    // 通信
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "UTF-8");
                    string url = "http://hatenaproxy.azurewebsites.net/api/timeline?user=" + m_myInfo.Id;
                    string json = await client.GetStringAsync(url);

                    // パース
                    TimelineResponse response = JsonConvert.DeserializeObject<TimelineResponse>(json);
                    if (!string.IsNullOrEmpty(response.Error)) throw new Exception(response.Error);
                    if (response.Result != "OK") throw new Exception("不明なエラー");

                    // 適用
                    m_list.Reset(response.Timeline);
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
            }
            // using 抜けたらローディング表示消える


            // エラーが発生していたら表示
            if(error != "")
            {
                await DisplayAlert("エラー", "タイムライン取得時にエラーが発生しました\n\n" + error, "OK");
            }

            // 処理終わったらボタン有効
            RefreshButton.IsEnabled = true;
        }

        bool m_loginProceeding = false;
        async Task AuthButton_Clicked(object sender, EventArgs e)
        {
            if (m_loginProceeding) return;
            m_loginProceeding = true;


            // ローディング表示
            var config = new ProgressDialogConfig()
                .SetTitle("Login ...")
                .SetIsDeterministic(false)
                .SetMaskType(MaskType.Black);
            string error = "";
            using (UserDialogs.Instance.Progress(config))
            {
                await Task.Delay(500);
                // ログイン
                try
                {
                    string resultJson = await StartAuth();

                    if (!resultJson.StartsWith("{"))
                    {
                        error = resultJson;
                    }
                    else
                    {
                        // JSONパース
                        try
                        {
                            // {"profile_image_url":"http://cdn1......gif?111111", "url_name":"kobake", "display_name": "kobake"}
                            Dictionary<string, string> mymap = JsonConvert.DeserializeObject<Dictionary<string, string>>(resultJson);
                            m_myInfo.Id = mymap["url_name"];
                            m_myInfo.DisplayName = mymap["display_name"];
                            m_myInfo.Image = mymap["profile_image_url"];
                        }
                        catch (Exception)
                        {
                            throw new Exception("不正なサーバ応答");
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
            }

            m_loginProceeding = false;

            // 結果表示
            if (!string.IsNullOrEmpty(error))
            {
                await DisplayAlert("エラー", "ログイン中にエラーが発生しました\n\n" + error, "OK");
            }
        }


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
            try
            {
                // initialize computehash function
                OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    return hash_hmacSha1(buffer, key);
                };

                var accessToken = await HatenaLogin.Authorize();
                if (accessToken == null) return "login error";

                m_client = new HatenaClient(accessToken);

                var my = await m_client.GetMy();
                Debug.WriteLine("my = " + my);

                return my;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
