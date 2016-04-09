using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace DmhyRssReader
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static readonly string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2679.0 Safari/537.36";
        private static Object LockdownloadListAll = new Object();
        private static readonly int MaxThreadNumber = 10;
        private static readonly string DefaultExplorer = "chrome.exe";
        private readonly MainWindowViewModel viewModel;
        private CustomDialog rssDialog;
        private CustomDialog proxyDialog;
        private Database database;
        private Hashtable downloadListAll;
        private bool useSystemProxy;
        private string proxyServer;
        private int proxyPort;

        public MainWindow()
        {
            this.database = new Database();
            this.viewModel = new MainWindowViewModel(this.database);
            this.DataContext = this.viewModel;

            InitializeComponent();

            this.rssDialog = this.Resources["RssDialog"] as CustomDialog;
            this.proxyDialog = this.Resources["ProxyDialog"] as CustomDialog;
            this.downloadListAll = new Hashtable();
            this.comboBoxDownloadList.SelectionChanged += comboBoxDownloadList_SelectionChanged;
            this.useSystemProxy = this.viewModel.UseSystemProxy;
        }

        private async void buttonAddRss_Click(object sender, RoutedEventArgs e)
        {
            this.rssDialog.Title = "添加RSS源";
            await this.ShowMetroDialogAsync(this.rssDialog);

            Button buttonOK = rssDialog.FindChild<Button>("buttonOK");
            Button buttonCancel = rssDialog.FindChild<Button>("buttonCancel");
            TextBox textBoxRssName = rssDialog.FindChild<TextBox>("textBoxRssName");
            TextBox textBoxRssUrl = rssDialog.FindChild<TextBox>("textBoxRssUrl");

            RssListBinding rlb = new RssListBinding();
            textBoxRssName.Text = rlb.Name;
            textBoxRssUrl.Text = rlb.Name;

            RoutedEventHandler addRssDialogButtonOKClick = null;
            RoutedEventHandler addRssDialogButtonCancelClick = null;
            addRssDialogButtonOKClick = async (o, args) =>
             {
                 buttonOK.Click -= addRssDialogButtonOKClick;
                 buttonCancel.Click -= addRssDialogButtonCancelClick;

                 rlb.Name = textBoxRssName.Text;
                 rlb.URL = textBoxRssUrl.Text;
                 rlb.Selected = true;
                 if (this.viewModel.RssList.Contains(rlb))
                 {
                     this.viewModel.RssList.Remove(rlb);
                     this.viewModel.RssListEx.Remove(rlb);
                     this.database.RemoveRssList(rlb);
                 }

                 this.viewModel.RssList.Add(rlb);
                 this.viewModel.RssListEx.Add(rlb);
                 this.database.AddRssList(rlb);

                 await this.HideMetroDialogAsync(this.rssDialog);
             };
            addRssDialogButtonCancelClick = async (o, args) =>
            {
                buttonOK.Click -= addRssDialogButtonOKClick;
                buttonCancel.Click -= addRssDialogButtonCancelClick;

                await this.HideMetroDialogAsync(this.rssDialog);
            };

            buttonOK.Click += addRssDialogButtonOKClick;
            buttonCancel.Click += addRssDialogButtonCancelClick;
        }

        private async void buttonEditRss_Click(object sender, RoutedEventArgs e)
        {
            RssListBinding rlb = this.dataGridRssList.SelectedItem as RssListBinding;
            if (rlb == null)
            {
                return;
            }

            this.rssDialog.Title = "编辑RSS源";
            await this.ShowMetroDialogAsync(this.rssDialog);

            Button buttonOK = rssDialog.FindChild<Button>("buttonOK");
            Button buttonCancel = rssDialog.FindChild<Button>("buttonCancel");
            TextBox textBoxRssName = rssDialog.FindChild<TextBox>("textBoxRssName");
            TextBox textBoxRssUrl = rssDialog.FindChild<TextBox>("textBoxRssUrl");

            textBoxRssName.Text = rlb.Name;
            textBoxRssUrl.Text = rlb.URL;

            RoutedEventHandler addRssDialogButtonOKClick = null;
            RoutedEventHandler addRssDialogButtonCancelClick = null;
            addRssDialogButtonOKClick = async (o, args) =>
            {
                buttonOK.Click -= addRssDialogButtonOKClick;
                buttonCancel.Click -= addRssDialogButtonCancelClick;

                this.database.RemoveRssList(rlb);

                rlb.Name = textBoxRssName.Text;
                rlb.URL = textBoxRssUrl.Text;

                this.database.AddRssList(rlb);

                await this.HideMetroDialogAsync(this.rssDialog);
            };
            addRssDialogButtonCancelClick = async (o, args) =>
            {
                buttonOK.Click -= addRssDialogButtonOKClick;
                buttonCancel.Click -= addRssDialogButtonCancelClick;

                await this.HideMetroDialogAsync(this.rssDialog);
            };

            buttonOK.Click += addRssDialogButtonOKClick;
            buttonCancel.Click += addRssDialogButtonCancelClick;

        }

        private void buttonDeleteRss_Click(object sender, RoutedEventArgs e)
        {
            RssListBinding rlb = this.dataGridRssList.SelectedItem as RssListBinding;
            if (rlb == null)
            {
                return;
            }
            this.viewModel.RssList.Remove(rlb);
            this.viewModel.RssListEx.Remove(rlb);
            this.database.RemoveRssList(rlb);
        }

        private async void buttonUpdateDownloadlist_Click(object sender, RoutedEventArgs e)
        {
            ProgressDialogController pdc = await this.ShowProgressAsync("更新下载列表", "操作进行中", false);
            pdc.SetIndeterminate();

            this.downloadListAll.Clear();

            List<Task> taskList = new List<Task>();

            foreach (RssListBinding rlb in this.viewModel.RssList)
            {
                if (rlb.Selected)
                {
                    if (taskList.Count < MaxThreadNumber)
                    {
                        taskList.Add(GetRSS(rlb));
                    }
                    else
                    {
                        await Task.WhenAny(taskList);
                        taskList.Add(GetRSS(rlb));
                    }
                }
            }

            await Task.WhenAll(taskList);

            UpdateDataGridDownloadList();

            await pdc.CloseAsync();
        }

        private async Task GetRSS(RssListBinding rlb)
        {
            this.database.UpdateRssList(rlb);

            HttpWebRequest httpWebRequest = HttpWebRequest.Create(rlb.URL) as HttpWebRequest;
            httpWebRequest.UserAgent = UserAgent;

            if (this.useSystemProxy)
            {
                httpWebRequest.Proxy = WebRequest.GetSystemWebProxy();
            }
            else
            {
                httpWebRequest.Proxy = new WebProxy(this.proxyServer, this.proxyPort);
            }

            using (HttpWebResponse httpWebResponse = (await httpWebRequest.GetResponseAsync()) as HttpWebResponse)
            {
                using (Stream responseStream = httpWebResponse.GetResponseStream())
                {
                    using (XmlReader xmlReader = XmlReader.Create(responseStream))
                    {
                        SyndicationFeed syndicationFeed = SyndicationFeed.Load(xmlReader);

                        DateTime lastUpdateTime = rlb.UpdateTimeValue;

                        foreach (SyndicationItem si in syndicationFeed.Items)
                        {
                            DownloadListBinding dlb = new DownloadListBinding();
                            dlb.RSS = rlb;
                            dlb.Title = si.Title.Text;
                            dlb.UpdateTimeValue = si.PublishDate.DateTime;
                            foreach (SyndicationLink sl in si.Links)
                            {
                                if (sl.MediaType != null && sl.MediaType.Equals("application/x-bittorrent"))
                                {
                                    dlb.MagnetLink = sl.Uri.AbsoluteUri;
                                }
                            }
                            dlb.GUID = si.Id;
                            dlb.Downloaded = this.database.IsDownloaded(dlb);
                            dlb.Selected = !dlb.Downloaded;
                            lock (LockdownloadListAll)
                            {
                                if (!this.downloadListAll.Contains(dlb.MD5))
                                {
                                    this.downloadListAll.Add(dlb.MD5, dlb);
                                }
                            }
                            if (dlb.UpdateTimeValue > lastUpdateTime)
                            {
                                lastUpdateTime = dlb.UpdateTimeValue;
                            }
                        }

                        rlb.UpdateTimeValue = lastUpdateTime;
                        this.database.UpdateRssList(rlb);
                    }
                }
            }
        }

        private async void buttonSaveDownloadList_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "选择保存位置";
            dialog.Filter = "TXT文件(*.txt)|*.txt";
            if (dialog.ShowDialog() == true)
            {
                ProgressDialogController pdc = await this.ShowProgressAsync("导出下载列表", "操作进行中", false);
                pdc.SetIndeterminate();
                using (MemoryStream ms = new MemoryStream())
                {
                    foreach (DictionaryEntry de in this.downloadListAll)
                    {
                        DownloadListBinding dlb = de.Value as DownloadListBinding;
                        if (dlb.Selected)
                        {
                            byte[] tmpData = System.Text.Encoding.Default.GetBytes(dlb.MagnetLink + Environment.NewLine);
                            await ms.WriteAsync(tmpData, 0, tmpData.Length).ConfigureAwait(false);
                            if (!dlb.Downloaded)
                            {
                                await Task.Run(new Action(() => this.database.AddDownloadedList(dlb))).ConfigureAwait(false);
                            }
                            dlb.Downloaded = true;
                        }
                    }

                    string tmpStr = System.Text.Encoding.Default.GetString(ms.ToArray());
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        Clipboard.Clear();
                        Clipboard.SetData(DataFormats.Text, tmpStr);
                    }));

                    using (FileStream fs = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        await fs.WriteAsync(ms.ToArray(), 0, (int)ms.Length).ConfigureAwait(false);
                    }
                }
                await pdc.CloseAsync();
            }
        }

        private void comboBoxDownloadList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDataGridDownloadList();
        }

        private void UpdateDataGridDownloadList()
        {
            this.viewModel.DownloadList.Clear();

            foreach (DictionaryEntry de in this.downloadListAll)
            {
                DownloadListBinding dlb = de.Value as DownloadListBinding;
                if (this.viewModel.SelectedRss.Equals(RssListBinding.SpecialRss))
                {
                    this.viewModel.DownloadList.Add(dlb);
                }
                else
                {
                    if (dlb.RSS.Equals(this.viewModel.SelectedRss))
                    {
                        this.viewModel.DownloadList.Add(dlb);
                    }
                }
            }
        }

        private static class CustomAppTheme
        {
            private static readonly Queue<string> color = new Queue<string>
            (
                new string[] { "Red", "Green", "Purple", "Orange",
                "Lime", "Emerald", "Teal", "Cyan", "Cobalt",
                "Indigo", "Violet", "Pink", "Magenta", "Crimson",
                "Amber", "Yellow", "Brown", "Olive", "Steel",
                "Mauve", "Taupe", "Sienna" }
            );

            public static string GetNextColor(string current)
            {
                color.Enqueue(current);
                return color.Dequeue();
            }
        }

        private void ButtonTheme_Click(object sender, RoutedEventArgs e)
        {
            Tuple<AppTheme, Accent> appStyle = ThemeManager.DetectAppStyle(Application.Current);
            string color = appStyle.Item2.Name;
            ThemeManager.ChangeAppStyle(Application.Current,
                                    ThemeManager.GetAccent(CustomAppTheme.GetNextColor(color)),
                                    ThemeManager.GetAppTheme("BaseLight"));
        }

        private async void ButtonProxy_Click(object sender, RoutedEventArgs e)
        {
            this.proxyDialog.Title = "代理服务器设置";
            await this.ShowMetroDialogAsync(this.proxyDialog);

            Button buttonProxyOK = proxyDialog.FindChild<Button>("buttonProxyOK");
            Button buttonProxyCancel = proxyDialog.FindChild<Button>("buttonProxyCancel");
            TextBox textBoxProxyServer = proxyDialog.FindChild<TextBox>("textBoxProxyServer");
            TextBox textBoxProxyPort = proxyDialog.FindChild<TextBox>("textBoxProxyPort");

            this.viewModel.UseSystemProxy = this.useSystemProxy;
            if (!this.useSystemProxy)
            {
                textBoxProxyServer.Text = this.proxyServer;
                textBoxProxyPort.Text = this.proxyPort.ToString();
            }

            RoutedEventHandler proxyDialogButtonOKClick = null;
            RoutedEventHandler proxyDialogButtonCancelClick = null;
            proxyDialogButtonOKClick = async (o, args) =>
            {
                buttonProxyOK.Click -= proxyDialogButtonOKClick;
                buttonProxyCancel.Click -= proxyDialogButtonCancelClick;

                this.useSystemProxy = this.viewModel.UseSystemProxy;

                if (!this.viewModel.UseSystemProxy)
                {
                    this.proxyServer = textBoxProxyServer.Text;
                    this.proxyPort = Convert.ToInt32(textBoxProxyPort.Text);
                }

                await this.HideMetroDialogAsync(this.proxyDialog);
            };
            proxyDialogButtonCancelClick = async (o, args) =>
            {
                buttonProxyOK.Click -= proxyDialogButtonOKClick;
                buttonProxyCancel.Click -= proxyDialogButtonCancelClick;

                await this.HideMetroDialogAsync(this.proxyDialog);
            };

            buttonProxyOK.Click += proxyDialogButtonOKClick;
            buttonProxyCancel.Click += proxyDialogButtonCancelClick;
        }

        private void dataGridDownloadList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DownloadListBinding dlb = (sender as DataGrid).SelectedItem as DownloadListBinding;
            if (dlb != null)
            {
                try
                {
                    Process.Start(DefaultExplorer, dlb.GUID);
                }
                catch
                {
                    Process.Start(dlb.GUID);
                }
            }
        }
    }
}
