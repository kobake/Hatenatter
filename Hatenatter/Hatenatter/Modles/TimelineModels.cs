using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hatenatter.Models
{
    public class HatenaComment
    {
        public string UserId { get; set; }
        public string Comment { get; set; }

        public string UserImage
        {
            get
            {
                if (UserId.Length < 2) return "";
                return $"http://cdn1.www.st-hatena.com/users/{UserId.Substring(0, 2)}/{UserId}/profile.gif";
            }
        }

        private string date;
        public string Date
        {
            get { return date; }
            set
            {
                date = value.Trim();
                try
                {
                    string[] tmp = date.Split('/');
                    date = string.Format("{0:D4}/{1:D2}/{2:D2}", int.Parse(tmp[0]), int.Parse(tmp[1]), int.Parse(tmp[2])); // YYYY/MM/DD に揃える
                }
                catch (Exception)
                {
                    // 例外が生じた場合はそのまま使う
                }
            }
        }
    }
    public class TimelineItem
    {
        public string ArticleName { get; set; }
        public string ArticleUrl { get; set; }
        public string ArticleId { get; set; }

        // "http://b.hatena.ne.jp/entry.touch/s/qiita.com/" のような形式
        public string HatenaUrl
        {
            get
            {
                if (string.IsNullOrEmpty(ArticleUrl)) return "";

                string url = ArticleUrl;
                if (ArticleUrl.StartsWith("http://"))
                {
                    url = url.Substring("http://".Length);
                    url = "http://b.hatena.ne.jp/entry.touch/" + url;
                }
                else if (ArticleUrl.StartsWith("https://"))
                {
                    url = url.Substring("https://".Length);
                    url = "http://b.hatena.ne.jp/entry.touch/s/" + url;
                }
                else
                {
                    url = "http://b.hatena.ne.jp/entry.touch/" + url;
                }
                return url;
            }
        }
        // "http://b.hatena.ne.jp/entry/52080118/comment/hxmasaki" のような形式
        public string HatenaCommentUrl
        {
            get
            {
                if (string.IsNullOrEmpty(ArticleId)) return "";

                string url = $"http://b.hatena.ne.jp/entry/{ArticleId}/comment/{Comment.UserId}";
                return url;
            }
        }


        public int BookmarkCount { get; set; }

        public HatenaComment Comment { get; set; }
    }
}