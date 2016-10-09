using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Hatenatter
{
    public partial class Page1 : ContentPage
    {
        Random m_random = new Random();

        // 指定ワードをランダム回繰り返した文字列を作る
        private string RandomDup(string word)
        {
            string ret = word;
            int n = m_random.Next() % 10;
            for (int i = 0; i < n; i++)
            {
                ret += "\n" + word;
            }
            return ret;
        }

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
                    State = RandomDup("Hello")
                }
            );
            list.Add(
                new ItemInfo
                {
                    Image = "https://avatars0.githubusercontent.com/u/20608487?v=3&s=200",
                    Name = "Kei",
                    State = RandomDup("こんにちは")
                }
            );
            list.Add(
                new ItemInfo
                {
                    Image = "https://avatars0.githubusercontent.com/u/20608487?v=3&s=200",
                    Name = "Tama",
                    State = RandomDup("(◔౪◔)!!!")
                }
            );

            // Bind
            this.BindingContext = list;
        }
    }
}
