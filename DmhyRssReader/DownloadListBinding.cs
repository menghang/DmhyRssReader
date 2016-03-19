using System.ComponentModel;

namespace DmhyRssReader
{
    class DownloadListBinding : INotifyPropertyChanged
    {
        private RssListBinding rss;
        public RssListBinding RSS
        {
            get
            {
                return this.rss;
            }
            set
            {
                this.rss = value;
                this.OnPropertyChanged("RSS");
            }
        }

        private string title;
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
                this.OnPropertyChanged("Title");
            }
        }

        private string updateTime;
        public string Updatetime
        {
            get
            {
                return this.updateTime;
            }
            set
            {
                this.updateTime = value;
                this.OnPropertyChanged("UpdateTime");
            }
        }

        private string magnetLink;
        public string Magnetlink
        {
            get
            {
                return this.magnetLink;
            }
            set
            {
                this.magnetLink = value;
                this.OnPropertyChanged("MagnetLink");
            }
        }

        private bool downloaded;
        public bool Downloaded
        {
            get
            {
                return this.downloaded;
            }
            set
            {
                this.downloaded = value;
                this.OnPropertyChanged("Downloaded");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
