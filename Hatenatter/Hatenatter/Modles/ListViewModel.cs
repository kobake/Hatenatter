﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hatenatter.Modles
{

    public class ListViewModel : INotifyPropertyChanged
    {
        List<ItemInfo> list = new List<ItemInfo>();
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
        public List<ItemInfo> MyListData
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

        public void Add(ItemInfo item)
        {
            list.Add(item);
            OnPropertyChanged("MyListData");
        }
        

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
}
