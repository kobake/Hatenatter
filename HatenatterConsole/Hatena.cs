using AsyncOAuth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HatenatterConsole
{
    // a sample of hatena client
    public class HatenaClient
    {
        readonly string consumerKey;
        readonly string consumerSecret;
        readonly AccessToken accessToken;

        public HatenaClient(string consumerKey, string consumerSecret, AccessToken accessToken)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            this.accessToken = accessToken;
        }

        // sample flow for Hatena authroize
        public async static Task<AccessToken> AuthorizeSampleRedirect(string consumerKey, string consumerSecret)
        {
            // create authorizer
            var authorizer = new OAuthAuthorizer(consumerKey, consumerSecret);

            // get request token
            var tokenResponse = await authorizer.GetRequestToken(
                "https://www.hatena.com/oauth/initiate",
                new[] { new KeyValuePair<string, string>("oauth_callback", "http://dev.clock-up.jp/redirect_to_app.php") },
                new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("scope", "read_public") }));
            var requestToken = tokenResponse.Token;

            // 

            // ブラウザ起動
            var pinRequestUrl = authorizer.BuildAuthorizeUrl("https://www.hatena.ne.jp/oauth/authorize", requestToken);
            Process.Start(pinRequestUrl);

            // 認証が成功すると、
            // http://dev.clock-up.jp/redirect_to_app.php?oauth_token=LjjaOIMY8fXTAA%3D%3D&oauth_verifier=77EbWdvM9KWG1Tjct%2FphLI%2Fp
            // のような形式でモノが来る

            // query to verifier -> verifier
            Console.WriteLine("ENTER QUERY PARAMETERS");
            var query = Console.ReadLine();
            string[] parameters = query.Split('&');
            string verifier = "";
            foreach (var p in parameters)
            {
                if (p.StartsWith("oauth_verifier="))
                {
                    verifier = p.Substring("oauth_verifier=".Length);
                }
            }
            var accessTokenResponse = await authorizer.GetAccessToken("https://www.hatena.com/oauth/token", requestToken, verifier);
            // get access token
            // var accessTokenResponse = await authorizer.GetAccessToken("https://www.hatena.com/oauth/token", requestToken, pinCode);

            // save access token.
            var accessToken = accessTokenResponse.Token;
            Console.WriteLine("Key:" + accessToken.Key);
            Console.WriteLine("Secret:" + accessToken.Secret);

            return accessToken;
        }

        // sample flow for Hatena authroize
        public async static Task<AccessToken> AuthorizeSamplePin(string consumerKey, string consumerSecret)
        {
            // create authorizer
            var authorizer = new OAuthAuthorizer(consumerKey, consumerSecret);

            // get request token
            var tokenResponse = await authorizer.GetRequestToken(
                "https://www.hatena.com/oauth/initiate",
                new[] { new KeyValuePair<string, string>("oauth_callback", "oob") },
                new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("scope", "read_public") }));
            var requestToken = tokenResponse.Token;

            // 

            var pinRequestUrl = authorizer.BuildAuthorizeUrl("https://www.hatena.ne.jp/oauth/authorize", requestToken);

            // open browser and get PIN Code
            Process.Start(pinRequestUrl);

            // enter pin
            Console.WriteLine("ENTER PIN");
            var pinCode = Console.ReadLine();

            // get access token
            var accessTokenResponse = await authorizer.GetAccessToken("https://www.hatena.com/oauth/token", requestToken, pinCode);

            // save access token.
            var accessToken = accessTokenResponse.Token;
            Console.WriteLine("Key:" + accessToken.Key);
            Console.WriteLine("Secret:" + accessToken.Secret);

            return accessToken;
        }

        HttpClient CreateOAuthClient()
        {
            return OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);
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