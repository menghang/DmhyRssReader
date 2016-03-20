using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DmhyRssReader
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<RssListBinding> RssList
        {
            get;
            set;
        }

        public ObservableCollection<RssListBinding> RssListEx
        {
            get;
            set;
        }
        
        public ObservableCollection<DownloadListBinding> DownloadList
        {
            get;
            set;
        }
        private RssListBinding selectedRss;
        public RssListBinding SelectedRss
        {
            private set
            {
                this.selectedRss = value;
                DownloadListBinding.SelectedRss = this.selectedRss;
                //RaisePropertyChanged("DownloadList");
            }
            get
            {
                return this.selectedRss;
            }
        }

        public MainWindowViewModel(Database _database)
        {
            this.RssList = _database.DatabaseInitialize();
            this.DownloadList = new ObservableCollection<DownloadListBinding>();
            this.RssListEx = new ObservableCollection<RssListBinding>(this.RssList);
            this.RssListEx.Insert(0, RssListBinding.SpecialRss);
            this.SelectedRss = RssListBinding.SpecialRss;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
