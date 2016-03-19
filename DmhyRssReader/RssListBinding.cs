using System;
using System.ComponentModel;

namespace DmhyRssReader
{
    public class RssListBinding : INotifyPropertyChanged
    {
        private bool seleted;
        public bool Seleted
        {
            get
            {
                return this.seleted;
            }
            set
            {
                this.seleted = value;
                this.OnPropertyChanged("Seleted");
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
                this.OnPropertyChanged("Name");
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
                this.OnPropertyChanged("URL");
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
                this.OnPropertyChanged("UpdateTime");
            }
        }
        public string UpdateTime
        {
            get
            {
                return this.updateTimeValue.ToString("f");
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
