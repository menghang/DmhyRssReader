using System;
using System.ComponentModel;
using System.Security.Cryptography;

namespace DmhyRssReader
{
    internal class DownloadListBinding : INotifyPropertyChanged, IEquatable<DownloadListBinding>
    {
        private bool selected;
        public bool Selected
        {
            get { return this.selected; }
            set
            {
                this.selected = value;
                RaisePropertyChanged(nameof(Selected));
            }
        }
        public RssListBinding RSS { get; set; }

        private string title;
        public string Title
        {
            get { return this.title; }
            set
            {
                this.title = value;
                RaisePropertyChanged(nameof(Title));
            }
        }

        private DateTime updateTimeValue;
        public DateTime UpdateTimeValue
        {
            get { return this.updateTimeValue; }
            set
            {
                this.updateTimeValue = value;
                RaisePropertyChanged(nameof(UpdateTime));
            }
        }
        public string UpdateTime
        {
            get { return this.updateTimeValue.ToString("f"); }
        }
        public string MagnetLink { get; set; }

        private bool downloaded;
        public bool Downloaded
        {
            get { return this.downloaded; }
            set
            {
                this.downloaded = value;
                RaisePropertyChanged(nameof(Downloaded));
            }
        }

        private string guid;
        public string GUID
        {
            get { return this.guid; }
            set { this.guid = value; }
        }

        public string MD5
        {
            get
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(this.GUID);
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

        public static RssListBinding SelectedRss;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Equals(DownloadListBinding other)
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
