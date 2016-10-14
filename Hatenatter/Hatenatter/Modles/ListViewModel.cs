using Hatenatter.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hatenatter.Modles
{
    /*
    public class ItemInfo : INotifyPropertyChanged
    {
        public string Image { get; set; }
        public string Name { get; set; }
        public string State { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    */

    // http://ytabuchi.hatenablog.com/entry/2015/08/12/010522
    public class ListViewModel : INotifyCollectionChanged
    {
        //List<ItemInfo> list = new List<ItemInfo>();
        ObservableCollection<TimelineItem> list = new ObservableCollection<TimelineItem>();
        /*
        list.Add(
                new ItemInfo
                {
                    Image = "https://avatars0.githubusercontent.com/u/20608487?v=3&s=200",
                    Name = "John",
                    State = "Hello"
                }
            );
        */
        public ObservableCollection<TimelineItem> MyListData
        {
            get
            {
                return list;
            }
            set
            {
                if (list != value)
                {
                    list = value;
                    OnPropertyChanged("MyListData");
                }
            }
        }

        public void Add(TimelineItem item)
        {
            list.Add(item);
            //OnPropertyChanged("MyListData"); // これやんなくてもObservableCollectionなので自動的に反映される
        }

        public void Reset(IEnumerable<TimelineItem> items)
        {
            list.Clear();
            foreach (TimelineItem item in items)
            {
                list.Add(item);
            }
        }

        //### こういう変更は見た目に適用されないっぽ…？
        public void Change(string t)
        {
            list[0].Comment.UserId = t;
            OnPropertyChanged("MyListData");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
