using AsyncOAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HatenatterConsole
{
    class Program
    {
        const string CONSUMER_KEY = "OjrD3wav+EZbSw==";
        const string CONSUMER_SECRET = "yKV005kzISrG63/vk1l5mApZi8I=";


        // set your token
        const string consumerKey = "OjrD3wav+EZbSw==";
        const string consumerSecret = "yKV005kzISrG63/vk1l5mApZi8I=";

        static async Task Run()
        {
            // initialize computehash function
            OAuthUtility.ComputeHash = (key, buffer) => { using (var hmac = new HMACSHA1(key)) { return hmac.ComputeHash(buffer); } };

            // sample, twitter access flow
            var accessToken = await HatenaClient.AuthorizeSample(consumerKey, consumerSecret);

            var client = new HatenaClient(consumerKey, consumerSecret, accessToken);

            //var tl = await client.GetTimeline(10, 1);
            //Console.WriteLine(tl);
            var my = await client.GetMy();
            Console.WriteLine("my = " + my);
        }

        static void Main(string[] args)
        {
            Run().Wait();
        }



        static void Main2(string[] args)
        {
            Console.WriteLine("--------");
            Task.Run(async () =>
            {
                int n = await StartAuth();
                /*
                var twitter = new TwitterApi(
                    "YHLiiAcYskxyO1aJGg1DyUhCB",
                    "bLAj2ybkvnAeIoT3cqas9sIOO36IlMbvsZakE0KdNkNb7awbqs",
                    "115328417-sA6umXj4pnVv33PPcx4j4dOySHke9V6K737FP6NA",
                    "Raf7pNp54enjYp3S81Mg0CYnUkDr8Oy9nZnNP48QR6eGf"
                );
                var response = await twitter.Tweet("Test");
                Console.WriteLine("response = " + response);
                */
            }).Wait();
            Console.WriteLine("--------");
        }



        private static readonly string randomStringTable = "0123456789abcdefghijklmnopqrstuvwxyz";
        private static string RandomString(int length)
        {
            StringBuilder sb = new StringBuilder(length);
            Random r = new Random();
            for (int i = 0; i < length; i++)
            {
                int pos = r.Next(randomStringTable.Length);
                char c = randomStringTable[pos];
                sb.Append(c);
            }
            return sb.ToString();
        }

        // http://d.hatena.ne.jp/nojima718/20100129/1264792636
        private static string GenerateSignature(string tokenSecret, string httpMethod, string url, SortedDictionary<string, string> parameters)
        {
            string signatureBase = GenerateSignatureBase(httpMethod, url, parameters);
            HMACSHA1 hmacsha1 = new HMACSHA1();
            hmacsha1.Key = Encoding.ASCII.GetBytes(UrlEncode(CONSUMER_SECRET) + '&' + UrlEncode(tokenSecret));
            byte[] data = System.Text.Encoding.ASCII.GetBytes(signatureBase);
            byte[] hash = hmacsha1.ComputeHash(data);
            return Convert.ToBase64String(hash);
        }
        private static string GenerateSignatureBase(string httpMethod, string url, SortedDictionary<string, string> parameters)
        {
            StringBuilder result = new StringBuilder();
            result.Append(httpMethod);
            result.Append('&');
            result.Append(UrlEncode(url));
            result.Append('&');
            result.Append(UrlEncode(JoinParameters(parameters)));
            return result.ToString();
        }
        public static string UrlEncode(string value)
        {
            string unreserved = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
            StringBuilder result = new StringBuilder();
            byte[] data = Encoding.UTF8.GetBytes(value);
            foreach (byte b in data)
            {
                if (b < 0x80 && unreserved.IndexOf((char)b) != -1)
                    result.Append((char)b);
                else
                    result.Append('%' + String.Format("{0:X2}", (int)b));
            }
            return result.ToString();
        }
        private static string JoinParameters(IDictionary<string, string> parameters)
        {
            StringBuilder result = new StringBuilder();
            bool first = true;
            foreach (var parameter in parameters)
            {
                if (first)
                    first = false;
                else
                    result.Append('&');
                result.Append(parameter.Key);
                result.Append('=');
                result.Append(parameter.Value);
            }
            return result.ToString();
        }




        

        static async Task<int> StartAuth()
        {
            // RequestToken取得
            if (true)
            {
                /*
                // create authorizer
                var authorizer = new OAuthAuthorizer(consumerKey, consumerSecret);

                // get request token
                var tokenResponse = await authorizer.GetRequestToken(
                    "https://www.hatena.com/oauth/initiate",
                    new[] { new KeyValuePair<string, string>("oauth_callback", "oob") },
                    new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("scope", "read_public") }));
                var requestToken = tokenResponse.Token;
                */

                return 0;
            }
            // RequestToken取得
            if (false)
            {
                var client = new HttpClient();
                string url = "https://www.hatena.com/oauth/initiate";

                // 共通ヘッダ設定
                // http://stackoverflow.com/questions/14627399/setting-authorization-header-of-httpclient
                // http://stackoverflow.com/questions/19039450/adding-authorization-to-the-headers
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                //var headers = new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                //headers.
                //client.DefaultRequestHeaders.Authorization = headers;
                //client.DefaultRequestHeaders.Add("Authorization", "");
                // 各パラメータの意味は http://d.hatena.ne.jp/ash1taka/20120218/1329533902 が参考になる
                string authHeader = "";
                authHeader += "OAuth realm=";
                authHeader += ",oauth_callback=oob";
                authHeader += ",oauth_consumer_key={consumer_key}";
                authHeader += ",oauth_nonce={nonce}";
                authHeader += ",oauth_signature_method=HMAC-SHA1";
                authHeader += ",oauth_timestamp={timestamp}";
                authHeader += ",oauth_version=1.0";
                //authHeader += ",oauth_signature={signature}";

                // hatenaから発行されたconsumer_key
                authHeader = authHeader.Replace("{consumer_key}", UrlEncode(CONSUMER_KEY)); //encode必要？

                // nonce は適当なランダムな文字列
                var r = new Random();
                string nonce = RandomString(20);
                authHeader = authHeader.Replace("{nonce}", nonce);

                // timestamp は UNIXTIME
                Int32 timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                authHeader = authHeader.Replace("{timestamp}", "" + timestamp);


                // https://syncer.jp/how-to-make-signature-of-oauth-1
                // http://oshiete.goo.ne.jp/qa/3384201.html
                // http://d.hatena.ne.jp/teru_kusu/20110120/1295520678
                // rawurlencode(" "); ⇒ %20
                // urlencode(" "); ⇒ +
                // SortedDictionary <string, string> parameters = new SortedDictionary<string, string>();
                // parameters["scope"] = "read_public";
                string tokenSecret = ""; // 初めは決まってないので空
                string signature_key = UrlEncode(CONSUMER_SECRET) + "&" + UrlEncode(tokenSecret);
                string signature_data = "POST&" + UrlEncode(url) + "&" + UrlEncode("scope=read_public");
                signature_data = "POST&" + UrlEncode(url) + "&" + UrlEncode(authHeader);                

                byte[] data = System.Text.Encoding.ASCII.GetBytes(signature_data);
                HMACSHA1 hmacsha1 = new HMACSHA1();
                hmacsha1.Key = Encoding.ASCII.GetBytes(signature_key);
                byte[] hash = hmacsha1.ComputeHash(data);
                string signature = Convert.ToBase64String(hash);

                // signature適用
                authHeader += ",oauth_signature={signature}";
                authHeader = authHeader.Replace("{signature}", UrlEncode(signature));



                //string signature = GenerateSignature("", "POST", url, parameters);
                //string signature = 

                // signature = GenerateSignatureBase("POST", url, parameters);



                // authHeader = authHeader.Replace("{consumer_secret}", "yKV005kzISrG63/vk1l5mApZi8I="); //encode必要？
                Console.WriteLine("------- auth header ---------");
                Console.WriteLine(authHeader);
                Console.WriteLine("-----------------------------\n");
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", authHeader);
                // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Your Oauth token");
                client.DefaultRequestHeaders.Add("Authorization", authHeader);
                Console.WriteLine("------- all headers ---------");
                foreach (var h in client.DefaultRequestHeaders)
                {
                    Console.WriteLine(h.Key);
                    foreach(var v in h.Value)
                    {
                        Console.WriteLine("    " + v);
                    }
                }
                Console.WriteLine("-----------------------------\n");

                // 送信データ
                var content = new StringContent("scope=read_public");

                HttpResponseMessage response = await client.PostAsync(url, content);
                string body = await response.Content.ReadAsStringAsync();
                Console.WriteLine("body = " + body);
                return 0;
            }
        }
    }
}
