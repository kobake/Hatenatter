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

            // ログイン情報の復元 (今回はトークンいらないので ID 表示情報だけ保存するずるい方式)
            if (Application.Current.Properties.ContainsKey("MyInfo"))
            {
                try
                {
                    string json = Application.Current.Properties["MyInfo"] as string;
                    m_myInfo = JsonConvert.DeserializeObject<UserViewModel>(json);
                }
                catch (Exception)
                {
                }
            }
            if(m_myInfo == null)
            {
                m_myInfo = new UserViewModel { Id = "", DisplayName = "unknown", Image = "nologin.png" };
            }
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

            // 右上メニュー画像タップ
            var tapRecognizer = new TapGestureRecognizer
            {
                Command = new Command(async () => {
                    await OnMenuIconClicked();
                }),
                NumberOfTapsRequired = 1
            };
            MenuIcon.GestureRecognizers.Add(tapRecognizer);

            // リストビュータップ
            MyList.ItemTapped += OnItemTapped;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // ログイン済みの場合はすぐにタイムラインのロードを始める
            Task.Run(async () =>
            {
                await Task.Delay(50);
                Debug.WriteLine("=======Appear1");
                await Refresh();
                Debug.WriteLine("=======Appear2");
            });
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

        // 認証
        bool m_menuing = false;
        async Task OnMenuIconClicked()
        {
            if (m_menuing) return;
            m_menuing = true;

            // メニュー
            /*
            CancellationToken cancel = new CancellationToken();
            var result = await UserDialogs.Instance.ActionSheetAsync(
                "Hatenatter 0.8.0", // title
                "Cancel",
                "Destroy", cancel, "ログイン", "バージョン", "Button3");
            this.Result(result);
            */
            string loginOrLogout = "ログイン";
            if (!string.IsNullOrEmpty(m_myInfo.Id)) loginOrLogout = "ログアウト";
            var action = await DisplayActionSheet(
                "Hatenatter 0.8.0", "Cancel", "Delete",
                loginOrLogout // 可変個引数で選択肢増やせるが、今のところは「ログイン/ログアウト」しか必要ないのでこれだけ。
            );
            if(action == "ログイン")
            {
                Debug.WriteLine("========================LoginA");
                await AuthMenu_Clicked();
                Debug.WriteLine("========================LoginB");
            }
            else if(action == "ログアウト")
            {
                Application.Current.Properties["MyInfo"] = "";
                m_myInfo = new UserViewModel { Id = "", DisplayName = "unknown", Image = "nologin.png" };
                MyUserLayout.BindingContext = m_myInfo;
                m_list.Clear();
            }
            else
            {
                // 何もしない
            }
            m_menuing = false;
        }

        // タイムライン更新
        async void RefreshButton_Clicked(object sender, EventArgs e)
        {
            await Refresh();
        }
        async Task Refresh()
        {
            // ログインしてない場合は何もしない
            if (string.IsNullOrEmpty(m_myInfo.Id))
            {
                Debug.WriteLine("Error: required to login");
                return;
            }

            // 処理中はボタン無効
            if (!RefreshButton.IsEnabled) return;
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
        async Task AuthMenu_Clicked()
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
            else
            {
                string json = JsonConvert.SerializeObject(m_myInfo);
                Application.Current.Properties["MyInfo"] = json;

                // すぐにタイムラインのロードを始める
                await Task.Delay(50);
                Debug.WriteLine("=======RefreshA");
                await Refresh();
                Debug.WriteLine("=======RefreshB");
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
