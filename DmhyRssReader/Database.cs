using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DmhyRssReader
{
    public class Database
    {
        private const string DatabaseFile = "data.db";

        private static Object lockThis = new Object();

        public ObservableCollection<RssListBinding> DatabaseInitialize()
        {
            lock (lockThis)
            {
                if (DatabaseBroken())
                {
                    CreateNewDataBase();
                    return new ObservableCollection<RssListBinding>();
                }
                else
                {
                    return GetRssListFromDatabase();
                }
            }
        }

        private bool DatabaseBroken()
        {
            if (!File.Exists(DatabaseFile))
            {
                return true;
            }

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + DatabaseFile))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    SQLiteHelper sh = new SQLiteHelper(cmd);

                    DataTable dt = sh.GetTableStatus();
                    bool rssListExist = false;
                    bool downloadedListExist = false;
                    foreach (DataRow dr in dt.Rows)
                    {
                        if ((dr["type"] as string).Equals("table") && (dr["name"] as string).Equals("RssList"))
                        {
                            rssListExist = true;
                            continue;
                        }
                        if ((dr["type"] as string).Equals("table") && (dr["name"] as string).Equals("DownloadedList"))
                        {
                            downloadedListExist = true;
                            continue;
                        }
                    }
                    if (!rssListExist || !downloadedListExist)
                    {
                        conn.Close();
                        return true;
                    }

                    DataTable dt2 = sh.GetColumnStatus("RssList");
                    bool[] flag1 = { false, false, false, false };

                    foreach (DataRow dr2 in dt2.Rows)
                    {
                        if ((dr2["type"] as string).Equals("text")
                            && (dr2["name"] as string).Equals("Name")
                            && Convert.ToInt32(dr2["pk"]) == 0)
                        {
                            flag1[0] = true;
                            continue;
                        }
                        if ((dr2["type"] as string).Equals("text")
                            && (dr2["name"] as string).Equals("Url")
                            && Convert.ToInt32(dr2["pk"]) == 0)
                        {
                            flag1[1] = true;
                            continue;
                        }
                        if ((dr2["type"] as string).Equals("datetime")
                            && (dr2["name"] as string).Equals("UpdateTime")
                            && Convert.ToInt32(dr2["pk"]) == 0)
                        {
                            flag1[2] = true;
                            continue;
                        }
                        if ((dr2["type"] as string).Equals("text")
                            && (dr2["name"] as string).Equals("Md5")
                            && Convert.ToInt32(dr2["pk"]) == 1)
                        {
                            flag1[3] = true;
                            continue;
                        }
                    }
                    if (!flag1[0] || !flag1[1] || !flag1[2] || !flag1[3])
                    {
                        conn.Close();
                        return true;
                    }

                    DataTable dt3 = sh.GetColumnStatus("DownloadedList");
                    bool[] flag2 = { false, false, false, false, false, false };

                    foreach (DataRow dr3 in dt3.Rows)
                    {
                        if ((dr3["type"] as string).Equals("text")
                            && (dr3["name"] as string).Equals("RssUrl")
                            && Convert.ToInt32(dr3["pk"]) == 0)
                        {
                            flag2[0] = true;
                            continue;
                        }
                        if ((dr3["type"] as string).Equals("text")
                            && (dr3["name"] as string).Equals("Title")
                            && Convert.ToInt32(dr3["pk"]) == 0)
                        {
                            flag2[1] = true;
                            continue;
                        }
                        if ((dr3["type"] as string).Equals("datetime")
                            && (dr3["name"] as string).Equals("UpdateTime")
                            && Convert.ToInt32(dr3["pk"]) == 0)
                        {
                            flag2[2] = true;
                            continue;
                        }
                        if ((dr3["type"] as string).Equals("text")
                            && (dr3["name"] as string).Equals("MagnetLink")
                            && Convert.ToInt32(dr3["pk"]) == 0)
                        {
                            flag2[3] = true;
                            continue;
                        }
                        if ((dr3["type"] as string).Equals("text")
                            && (dr3["name"] as string).Equals("Guid")
                            && Convert.ToInt32(dr3["pk"]) == 0)
                        {
                            flag2[4] = true;
                            continue;
                        }
                        if ((dr3["type"] as string).Equals("text")
                            && (dr3["name"] as string).Equals("Md5")
                            && Convert.ToInt32(dr3["pk"]) == 1)
                        {
                            flag2[5] = true;
                            continue;
                        }
                    }
                    if (!flag2[0] || !flag2[1] || !flag2[2] || !flag2[3] || !flag2[4] || !flag2[5])
                    {
                        conn.Close();
                        return true;
                    }
                    conn.Close();
                    return false;
                }
            }
        }

        private void CreateNewDataBase()
        {
            if (File.Exists(DatabaseFile))
            {
                File.Delete(DatabaseFile);
            }
            SQLiteConnection.CreateFile(DatabaseFile);
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + DatabaseFile))
            {
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    command.Connection = connection;
                    connection.Open();
                    SQLiteHelper helper = new SQLiteHelper(command);

                    SQLiteTable tb = new SQLiteTable("RssList");
                    tb.Columns.Add(new SQLiteColumn("Name", ColType.Text));
                    tb.Columns.Add(new SQLiteColumn("Url", ColType.Text));
                    tb.Columns.Add(new SQLiteColumn("UpdateTime", ColType.DateTime));
                    tb.Columns.Add(new SQLiteColumn("Md5", ColType.Text, true, false, true, null));
                    helper.CreateTable(tb);

                    SQLiteTable tb2 = new SQLiteTable("DownloadedList");
                    tb2.Columns.Add(new SQLiteColumn("RssUrl", ColType.Text));
                    tb2.Columns.Add(new SQLiteColumn("Title", ColType.Text));
                    tb2.Columns.Add(new SQLiteColumn("UpdateTime", ColType.DateTime));
                    tb2.Columns.Add(new SQLiteColumn("MagnetLink", ColType.Text));
                    tb2.Columns.Add(new SQLiteColumn("Guid"));
                    tb2.Columns.Add(new SQLiteColumn("Md5", ColType.Text, true, false, true, null));
                    helper.CreateTable(tb2);

                    connection.Close();
                }
            }
        }

        private ObservableCollection<RssListBinding> GetRssListFromDatabase()
        {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + DatabaseFile))
            {
                using (SQLiteCommand command = new SQLiteCommand())
                {
                    command.Connection = connection;
                    connection.Open();
                    SQLiteHelper helper = new SQLiteHelper(command);

                    string cmd = "select * from RssList;";
                    DataTable dt = helper.Select(cmd);
                    ObservableCollection<RssListBinding> rlbc = new ObservableCollection<RssListBinding>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        RssListBinding rlb = new RssListBinding();
                        rlb.Name = dr["Name"] as string;
                        rlb.Selected = true;
                        rlb.UpdateTimeValue = Convert.ToDateTime(dr["UpdateTime"]);
                        rlb.URL = dr["Url"] as string;
                        rlbc.Add(rlb);
                    }
                    connection.Close();
                    return rlbc;
                }
            }
        }

        public void AddRssList(RssListBinding rlb)
        {
            lock (lockThis)
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + DatabaseFile))
                {
                    using (SQLiteCommand command = new SQLiteCommand())
                    {
                        command.Connection = connection;
                        connection.Open();
                        SQLiteHelper helper = new SQLiteHelper(command);

                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["Name"] = rlb.Name;
                        dic["Url"] = rlb.URL;
                        dic["UpdateTime"] = rlb.UpdateTimeValue.ToString("s");
                        dic["Md5"] = rlb.MD5;

                        helper.Insert("RssList", dic);

                        connection.Close();
                    }
                }
            }
        }

        public void RemoveRssList(RssListBinding rlb)
        {
            lock (lockThis)
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + DatabaseFile))
                {
                    using (SQLiteCommand command = new SQLiteCommand())
                    {
                        command.Connection = connection;
                        connection.Open();
                        SQLiteHelper helper = new SQLiteHelper(command);

                        string cmd = "delete from RssList where Md5='" + rlb.MD5 + "';";
                        helper.Execute(cmd);

                        connection.Close();
                    }
                }
            }
        }

        public void UpdateRssListUpdateTime(RssListBinding rlb)
        {
            lock (lockThis)
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + DatabaseFile))
                {
                    using (SQLiteCommand command = new SQLiteCommand())
                    {
                        command.Connection = connection;
                        connection.Open();
                        SQLiteHelper helper = new SQLiteHelper(command);

                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["Name"] = rlb.Name;
                        dic["Url"] = rlb.URL;
                        dic["UpdateTime"] = rlb.UpdateTimeValue.ToString("s");
                        dic["Md5"] = rlb.MD5;

                        helper.Update("RssList", dic, "Md5", rlb.MD5);

                        connection.Close();
                    }
                }
            }
        }

        public bool IsDownloaded(DownloadListBinding dlb)
        {
            lock (lockThis)
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + DatabaseFile))
                {
                    using (SQLiteCommand command = new SQLiteCommand())
                    {
                        command.Connection = connection;
                        connection.Open();
                        SQLiteHelper helper = new SQLiteHelper(command);

                        string cmd = "select * from DownloadedList where Md5='" + dlb.MD5 + "';";
                        DataTable dt = helper.Select(cmd);
                        connection.Close();
                        if (dt.Rows.Count > 0)
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
        }

        public void AddDownloadedList(DownloadListBinding dlb)
        {
            lock (lockThis)
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + DatabaseFile))
                {
                    using (SQLiteCommand command = new SQLiteCommand())
                    {
                        command.Connection = connection;
                        connection.Open();
                        SQLiteHelper helper = new SQLiteHelper(command);

                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["RssUrl"] = dlb.RSS.URL;
                        dic["Title"] = dlb.Title;
                        dic["UpdateTime"] = dlb.UpdateTimeValue.ToString("s");
                        dic["MagnetLink"] = dlb.MagnetLink;
                        dic["Guid"] = dlb.GUID;
                        dic["Md5"] = dlb.MD5;

                        helper.Insert("DownloadedList", dic);

                        connection.Close();
                    }
                }
            }
        }

    }
}
