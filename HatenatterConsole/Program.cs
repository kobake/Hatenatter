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

        static void Main(string[] args)
        {
            Console.WriteLine("--------");
            Task.Run(async () =>
            {
                //int n = await StartAuth();
                var twitter = new TwitterApi(
                    "YHLiiAcYskxyO1aJGg1DyUhCB",
                    "bLAj2ybkvnAeIoT3cqas9sIOO36IlMbvsZakE0KdNkNb7awbqs",
                    "115328417-sA6umXj4pnVv33PPcx4j4dOySHke9V6K737FP6NA",
                    "Raf7pNp54enjYp3S81Mg0CYnUkDr8Oy9nZnNP48QR6eGf"
                );
                var response = await twitter.Tweet("Test");
                Console.WriteLine("response = " + response);
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
                // 各パラメータの意味は http://d.hatena.ne.jp/ash1taka/20120218/1329533902 が参考になる
                string authHeader = "";
                authHeader += "OAuth realm=\"\"";
                authHeader += ", oauth_callback=\"oob\"";
                authHeader += ", oauth_consumer_key=\"{consumer_key}\"";
                authHeader += ", oauth_nonce=\"{nonce}\"";
                authHeader += ", oauth_signature=\"{signature}\"";
                authHeader += ", oauth_signature_method=\"HMAC-SHA1\"";
                authHeader += ", oauth_timestamp=\"1291689730\"";
                authHeader += ", oauth_version=\"1.0\"";

                // hatenaから発行されたconsumer_key
                authHeader = authHeader.Replace("{consumer_key}", UrlEncode(CONSUMER_KEY)); //encode必要？

                // nonce は適当なランダムな文字列
                var r = new Random();
                string nonce = RandomString(20);
                authHeader = authHeader.Replace("{nonce}", nonce);

                // signature (HMAC=SHA1)
                //string signature = GenerateSignature(tokenSecret, "POST", "https://www.hatena....", parameters);


                // authHeader = authHeader.Replace("{consumer_secret}", "yKV005kzISrG63/vk1l5mApZi8I="); //encode必要？

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authHeader);

                // 送信データ
                var content = new StringContent("scope=read_public");

                await client.PostAsync(url, content);

                return 0;
            }
        }
    }
}
