using Community.CsharpSqlite;
using HathLibraries.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace HathLibraries
{
    public static class DatabaseHandler
    {
        private static Community.CsharpSqlite.Sqlite3.sqlite3 Database;
        private static object dbLock = new object();


        public static void Innitialize()
        {
            Sqlite3.Open(Configuration.Locations.Database, out Database);

            Sqlite3.exec(Database, "CREATE TABLE IF NOT EXISTS CacheIndex (fileid VARCHAR(70)  NOT NULL, lasthit INT UNSIGNED NOT NULL, filesize INT UNSIGNED NOT NULL, strlen TINYINT UNSIGNED NOT NULL, active BOOLEAN NOT NULL, PRIMARY KEY(fileid));", 0, 0, 0);
            Sqlite3.exec(Database, "CREATE INDEX IF NOT EXISTS Lasthit ON CacheIndex (lasthit DESC);", 0, 0, 0);
            Sqlite3.exec(Database, "CREATE TABLE IF NOT EXISTS StringVars (k VARCHAR(255) NOT NULL, v VARCHAR(255) NOT NULL, PRIMARY KEY(k));", 0, 0, 0);
            Sqlite3.exec(Database, "CREATE TABLE IF NOT EXISTS Stats (time INT UNSIGNED NOT NULL, bytes_sent INT UNSIGNED NOT NULL, bytes_received INT UNSIGNED NOT NULL, files_uploaded INT UNSIGNED NOT NULL, files_downloaded INT UNSIGNED NOT NULL, requests_received INT UNSIGNED NOT NULL, requests_sent INT UNSIGNED NOT NULL, PRIMARY KEY (time));", 0, 0, 0);
        }

        public static bool Close()
        {
            try { Sqlite3.Close(Database); }
            catch { return false; }

            return true;
        }


        public static class FileCache
        {
            private static List<EHFile> pendingfiles = new List<EHFile>();

            public static void RegisterPending(EHFile file)
            {
                pendingfiles.Add(file);
                Stats.Trigger.PendingFiles(pendingfiles.Count);

                if (pendingfiles.Count < 50 || (pendingfiles.Count % 10) == 1)
                    return;

                EHApiResponse response = EHApi.RegisterPending(pendingfiles.ToArray());
                if (response.Head == "OK")
                {
                    Log.Add(LogType.Info, "Notified EHApi about {0} new files", pendingfiles.Count);
                    pendingfiles.Clear();
                }
                else
                    Log.Add(LogType.Warning, "Failed EHApi file notification.");
            }

            public static void InsertIntoCache(string file)
            {
                EHFile fl = new EHFile(file);

                if (fl.Verify() && fl.Move())
                    InsertIntoCache(fl);
                else
                    File.Delete(fl.Location);
            }

            public static bool InsertIntoCache(EHFile file)
            {
                lock (dbLock)
                {
                    try
                    {
                        Sqlite3.exec(Database, string.Format("INSERT INTO CacheIndex (fileid, lasthit, filesize, strlen, active) VALUES (\"{0}\", {1}, {2}, {3}, {4})", file.Name, Helpers.UnixTime(), file.Size, file.Name.Length, 0), 0, 0, 0);
                        return true;
                    }
                    catch (Exception Ex)
                    {
                        Ex.Print(LogType.Error, true, false);
                        return false;
                    }
                }
            }

            public static string[] GetFileCache()
            {
                lock (dbLock)
                {
                    List<string> files = new List<string>();

                    try
                    {
                        Sqlite3.exec(Database, "SELECT * FROM CacheIndex", new Sqlite3.dxCallback((object a_param, long argc, object argv, object column) =>
                        {
                            string[] argvs = (string[])argv;

                            files.Add(argvs[0]);
                            return 0;
                        }), 0, 0);
                    }
                    catch { }

                    return files.ToArray();
                }
            }

            public static bool RemoveFile(string file)
            {
                lock (dbLock)
                {
                    try
                    {
                        Sqlite3.exec(Database, string.Format("DELETE FROM CacheIndex WHERE fileid = \"{0}\"", file), 0, 0, 0);
                        return true;
                    }
                    catch (Exception Ex)
                    {
                        Ex.Print();
                        return false;
                    }
                }
            }

            public static bool Download(EHUri eHUri, out EHFile ehf)
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        using (Stream rs = client.OpenRead(new Uri(eHUri.Url)))
                        {
                            MemoryStream ms = new MemoryStream();
                            int read = 0;
                            int download = 0;
                            byte[] buff = new byte[1024 * 8];

                            while ((read = rs.Read(buff, 0, buff.Length)) > 0)
                            {
                                ms.Write(buff, 0, read);
                                download += read;

                                Stats.Trigger.ByteReceived(read);
                            }

                            rs.Close();

                            string flloc = string.Format("{0}{1}", Configuration.Locations.Temp, eHUri.FileName);
                            FileStream fs = new FileStream(flloc, FileMode.Create, FileAccess.Write);
                            ms.WriteTo(fs);
                            fs.Close();

                            //File.WriteAllBytes(flloc, buff);
                            ehf = new EHFile(flloc);
                            if (!ehf.PreFail && ehf.Verify())
                            {
                                if (ehf.Move())
                                {
                                    InsertIntoCache(ehf);
                                    Stats.Trigger.FileDownload(download);

                                    return true;
                                }
                                else
                                    return false;
                            }
                            else
                            {
                                File.Delete(flloc);
                                return false;
                            }
                        }
                    }
                }
                catch (Exception Ex)
                {
                    ehf = null;
                    Ex.Print();
                    return false;
                }
            }

            public static bool Download(EHUri eHUri)
            {
                EHFile fl;
                return Download(eHUri, out fl);
            }

            public static void Verify(VerifyMethod Method = VerifyMethod.Fast)
            {
                if (Method == VerifyMethod.Fast)
                {
                    Log.Add(LogType.Info, "    Starting fast cache verify:");

                    string[] files = GetFileCache();
                    foreach (string file in files)
                    {
                        EHFile fl = new EHFile(EHFile.BuildExpectedLocation(file));
                        if (fl.PreFail)
                            Log.Add(LogType.Info, "        {0} {1}", fl.Hash, RemoveFile(file) ? "!~Yellow~Removed" : "~Red~Failed to remove");
                        else
                            Stats.Trigger.FileCache(fl.Size, false);
                    }

                    Log.Add(LogType.Info, "    ~Green~Finished cache verification.");
                    Log.Add();
                }
                else if (Method == VerifyMethod.Normal)
                {

                }
                else if (Method == VerifyMethod.Full)
                {
                    Log.LogPosition LogEntry;

                    Log.Add(LogType.Info, "    Starting cache verify:");

                    List<EHFile> toMove = new List<EHFile>();

                    string[] files = LocateAllFiles(Configuration.Locations.Cache).ToArray();
                    foreach (string file in files)
                    {
                        try
                        {
                            EHFile fl = new EHFile(file);
                            bool vstatus = fl.Verify();

                            LogEntry = Log.Add(LogType.Info, "        {0} {1}", !fl.PreFail ? fl.Hash : fl.Name, vstatus ? "!~Green~Ok" : "!~Red~Failed - ");
                            if (!vstatus)
                            {
                                try
                                {
                                    File.Delete(fl.Location);
                                    LogEntry.Append(LogType.Info, "!~Green~Deleted");
                                }
                                catch (Exception Ex)
                                {
                                    LogEntry.Append(LogType.Info, "~Red~Failed deletion");
                                    Ex.Print();
                                }
                            }
                            else if (vstatus && !fl.PreFail)
                                LogEntry.Append(LogType.Info, InsertIntoCache(fl) ? " !~Green~Added" : " ~Red~Insertion failed");

                            if (!fl.PreFail)
                            {
                                Log.Add(LogType.Debug, "          Hash: {0} - {1}", fl.ActualHash, fl.Hash == fl.ActualHash ? "Match" : "!~Red~MISSMATCH");
                                Log.Add(LogType.Debug, "          Size: {0} - {1}", fl.ActualSize, fl.Size == fl.ActualSize ? "Match" : "!~Red~MISSMATCH");
                                Log.Add(LogType.Debug, "         Width: {0}", fl.Width);
                                Log.Add(LogType.Debug, "        Height: {0}", fl.Height);
                                Log.Add(LogType.Debug, "        Format: {0}", fl.Format);
                            }

                            if (!vstatus && !fl.PreFail && fl.Location != fl.ExpectedLocation)
                                toMove.Add(fl);
                        }
                        catch (Exception Ex)
                        {
                            Ex.Print();
                        }
                    }

                    if (toMove.Count >= 1)
                    {
                        Log.Add();

                        LogEntry = Log.Add(LogType.Info, "    Moving incorrectly placed files:");
                        foreach (EHFile fl in toMove)
                        {
                            Log.Add(LogType.Info, "        {0} ", fl.Hash);
                            if (fl.Move())
                                LogEntry.Append(LogType.Info, "!~Green~Moved");
                            else
                                LogEntry.Append(LogType.Info, "!~Red~Failed");
                        }
                    }

                    List<string> trmdirs = GetEmptyDirectories(Configuration.Locations.Cache);
                    if (trmdirs.Count >= 1)
                    {
                        Log.Add();
                        Log.Add(LogType.Info, "    Removing empty directories...");
                    }

                    while (trmdirs.Count >= 1)
                    {
                        foreach (string di in trmdirs)
                        {
                            LogEntry = Log.Add(LogType.Info, "        {0} ", di);
                            try { Directory.Delete(di); }
                            catch (Exception Ex)
                            {
                                LogEntry.Append(LogType.Info, "!~Red~Fail");
                                Ex.Print();
                                continue;
                            }
                            LogEntry.Append(LogType.Info, "!~Green~Ok");
                        }

                        trmdirs = GetEmptyDirectories(Configuration.Locations.Cache);
                    }

                    Log.Add();
                    Log.Add(LogType.Info, "    ~Green~Finished cache verification.");
                    Log.Add();
                }
            }

            private static List<string> LocateAllFiles(string dir)
            {
                List<string> files = new List<string>();
                string[] dirs = Directory.GetDirectories(dir);
                foreach (string sdir in dirs)
                    files.AddRange(LocateAllFiles(sdir));

                string[] flis = Directory.GetFiles(dir);
                foreach (string fl in flis)
                    files.Add(fl);

                return files;
            }

            private static List<string> GetEmptyDirectories(string dir)
            {
                List<string> edirs = new List<string>();

                try
                {
                    string[] dirs = Directory.GetDirectories(dir);
                    foreach (string sdir in dirs)
                        edirs.AddRange(GetEmptyDirectories(sdir));

                    if (Directory.GetFiles(dir).Count() == 0 && dirs.Count() == 0)
                        edirs.Add(dir);
                }
                catch { }

                return edirs;
            }
        }

        public static class StatCache
        {
            private static byte Tick = 0;
            
            private static uint BytesSent = 0;
            private static uint BytesReceived = 0;

            private static uint RequestsSent = 0;
            private static uint RequestsReceived = 0;

            private static uint FilesUploaded = 0;
            private static uint FilesDownloaded = 0;

            public static void Update(uint bs, uint br, uint rs, uint rr, uint fu, uint fd)
            {
                BytesSent += bs;
                BytesReceived += br;

                RequestsSent += rs;
                RequestsReceived += rr;

                FilesUploaded += fu;
                FilesDownloaded += fd;

                if (++Tick >= 60)
                    StoreAndReset();
            }

            public static StatData GetInnitial()
            {
                StatData sd = new StatData();

                try
                {
                    Sqlite3.exec(Database, "SELECT SUM(bytes_sent), SUM(bytes_received), SUM(files_uploaded), SUM(files_downloaded), SUM(requests_received), SUM(requests_sent) FROM Stats", new Sqlite3.dxCallback((object a_param, long argc, object argv, object column) =>
                    {
                        string[] argvs = (string[])argv;
                        
                        sd.TotalBytesSent = ulong.Parse(argvs[0]);
                        sd.TotalBytesReceived = ulong.Parse(argvs[1]);

                        sd.TotalFilesUploaded = ulong.Parse(argvs[2]);
                        sd.TotalFilesDownloaded = ulong.Parse(argvs[3]);

                        sd.TotalHttpRequestsRevceived = uint.Parse(argvs[4]);
                        sd.TotalHttpRequestsSent = uint.Parse(argvs[5]);

                        return 0;
                    }), 0, 0);
                }
                catch { }

                return sd;
            }

            private static void StoreAndReset()
            {
                lock (dbLock)
                {
                    Sqlite3.exec(Database, string.Format
                        (
                            "INSERT INTO Stats (time, bytes_sent, bytes_received, files_uploaded, files_downloaded, requests_received, requests_sent) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",

                            Helpers.UnixTime(),
                            BytesSent,
                            BytesReceived,
                            FilesUploaded,
                            FilesDownloaded,
                            RequestsReceived,
                            RequestsSent
                        ), 0, 0, 0);


                    BytesSent = 0;
                    BytesReceived = 0;

                    RequestsSent = 0;
                    RequestsReceived = 0;

                    FilesUploaded = 0;
                    FilesDownloaded = 0;

                    Tick = 0;
                }
            }
        }
    }
}
