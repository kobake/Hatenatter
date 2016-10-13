using Acr.UserDialogs;
using AsyncOAuth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Hatena
{
    public class HatenaConfig
    {
        public const string CONSUMER_KEY = "OjrD3wav+EZbSw==";
        public const string CONSUMER_SECRET = "yKV005kzISrG63/vk1l5mApZi8I=";
    }

    public class HatenaLogin
    {
        private static string m_verifier = "";
        // sample flow for Hatena authroize
        public static async Task<AccessToken> Authorize()
        {
            m_verifier = "";

            var authorizer = new OAuthAuthorizer(HatenaConfig.CONSUMER_KEY, HatenaConfig.CONSUMER_SECRET);
            // get request token
            var tokenResponse = await authorizer.GetRequestToken(
                "https://www.hatena.com/oauth/initiate",
                new[] { new KeyValuePair<string, string>("oauth_callback", "http://dev.clock-up.jp/redirect_to_app.php") },
                new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("scope", "read_public") }));
            var requestToken = tokenResponse.Token;
            
            // ブラウザ起動
            var pinRequestUrl = authorizer.BuildAuthorizeUrl("https://www.hatena.ne.jp/oauth/authorize", requestToken);
            Device.OpenUri(new Uri(pinRequestUrl));

            // 認証が成功すると、
            // http://dev.clock-up.jp/redirect_to_app.php?oauth_token=LjjaOIMY8fXTAA%3D%3D&oauth_verifier=77EbWdvM9KWG1Tjct%2FphLI%2Fp
            // のようなページが表示される。結果、アプリに呼び戻される

            // verifier が来るまで待つ
            while (true)
            {
                if(m_verifier == "CANCEL")
                {
                    return null;
                }
                if(m_verifier != "")
                {
                    break;
                }
                await Task.Delay(500);
            }
            var accessTokenResponse = await authorizer.GetAccessToken("https://www.hatena.com/oauth/token", requestToken, m_verifier);
            
            // save access token.
            var accessToken = accessTokenResponse.Token;
            Debug.WriteLine("Key:" + accessToken.Key);
            Debug.WriteLine("Secret:" + accessToken.Secret);

            return accessToken;
        }

        // Androidからの通知
        public static void OnGotVerifier(string verifier)
        {
            m_verifier = verifier;
        }

        public static void Cancel()
        {
            m_verifier = "CANCEL";
        }
    }

    // a sample of hatena client
    public class HatenaClient
    {
        AccessToken accessToken = null;

        public HatenaClient(AccessToken accessToken)
        {
            this.accessToken = accessToken;
        }

        HttpClient CreateOAuthClient()
        {
            return OAuthUtility.CreateOAuthClient(HatenaConfig.CONSUMER_KEY, HatenaConfig.CONSUMER_SECRET, accessToken);
        }

        public async Task<string> GetMy()
        {
            var client = CreateOAuthClient();

            var json = await client.GetStringAsync("http://n.hatena.com/applications/my.json");
            return json;
        }

        public async Task<string> AppicationStart()
        {
            var client = CreateOAuthClient();

            var response = await client.PostAsync("http://n.hatena.com/applications/start", new StringContent("", Encoding.UTF8));
            return await response.Content.ReadAsStringAsync();
        }
    }
}