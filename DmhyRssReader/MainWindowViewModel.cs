using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DmhyRssReader
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<RssListBinding> RssList { get; set; }

        public ObservableCollection<RssListBinding> RssListEx { get; set; }

        public ObservableCollection<DownloadListBinding> DownloadList { get; set; }

        public RssListBinding SelectedRss { private set; get; }

        private bool useSystemProxy;
        public bool UseSystemProxy
        {
            get { return this.useSystemProxy; }
            set
            {
                this.useSystemProxy = value;
                RaisePropertyChanged(nameof(UseSystemProxy));
                RaisePropertyChanged(nameof(UseCustomProxy));
            }
        }
        public bool UseCustomProxy
        {
            get { return !this.useSystemProxy; }
        }

        public MainWindowViewModel(Database _database)
        {
            this.RssList = _database.DatabaseInitialize();
            this.DownloadList = new ObservableCollection<DownloadListBinding>();
            this.RssListEx = new ObservableCollection<RssListBinding>(this.RssList);
            this.RssListEx.Insert(0, RssListBinding.SpecialRss);
            this.SelectedRss = RssListBinding.SpecialRss;
            this.UseSystemProxy = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
