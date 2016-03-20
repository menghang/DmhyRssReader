using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows;

namespace DmhyRssReader
{
    public class RssListBinding : INotifyPropertyChanged, IEquatable<RssListBinding>
    {
        public static readonly RssListBinding SpecialRss
            = new RssListBinding()
            {
                Name = "全部项目",
                Selected = true,
                URL = ""
            };

        private bool selected;
        public bool Selected
        {
            get
            {
                return this.selected;
            }
            set
            {
                this.selected = value;
                RaisePropertyChanged("Selected");
                RaisePropertyChanged("Show");
            }
        }

        private string name;
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
                RaisePropertyChanged("Name");
            }
        }

        private string url;
        public string URL
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value;
                RaisePropertyChanged("URL");
            }
        }

        private DateTime updateTimeValue;
        public DateTime UpdateTimeValue
        {
            get
            {
                return this.updateTimeValue;
            }
            set
            {
                this.updateTimeValue = value;
                RaisePropertyChanged("UpdateTime");
            }
        }
        public string UpdateTime
        {
            get
            {
                return this.updateTimeValue.ToString("f");
            }
        }

        public string MD5
        {
            get
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(this.URL);
                byte[] byteHash = md5.ComputeHash(byteValue);
                md5.Clear();
                string sTemp = "";
                for (int i = 0; i < byteHash.Length; i++)
                {
                    sTemp += byteHash[i].ToString("X").PadLeft(2, '0');
                }
                return sTemp.ToLower();
            }
        }

        public Visibility Show
        {
            get
            {
                if (this.selected)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool Equals(RssListBinding other)
        {
            if (this.MD5.Equals(other.MD5))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
