using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HathLibraries.DataTypes
{
    public static class HttpServerCmd
    {
        public static ResourceFuncData CacheList(int FileSize, int FileCount)
        {
            string[] cache = DatabaseHandler.FileCache.GetFileCache();
            return new ResourceFuncData(Encoding.ASCII.GetBytes(string.Join("\n", cache)));
        }

        public static ResourceFuncData CacheFiles(string files)
        {
            string[] Files = files.Split(';');
            StringBuilder sb = new StringBuilder();

            Log.Add(LogType.Info, "Downloading files to cache:");

            List<Task> tasks = new List<Task>();

            foreach (string File in Files)
            {
                string[] pcs = File.Split(':');

                string FileName = pcs[0];
                string Host = pcs[1].Split('=')[0];
                string HostFile = pcs[1].Split('=')[1];

                Log.LogPosition LogEntry = Log.Add(LogType.Info, "    {0} ", FileName);

                tasks.Add(Task.Factory.StartNew(() =>
                {
                    if (DatabaseHandler.FileCache.Download(new EHUri(new string[] { Host, FileName, HostFile }, EHUriType.Download)))
                    {
                        LogEntry.Append(LogType.Info, "!~Green~Success");
                        sb.AppendFormat("{0}:OK\n", FileName);
                    }
                    else
                    {
                        LogEntry.Append(LogType.Info, "!~Yellow~Fail");
                        sb.AppendFormat("{0}:FAIL\n", FileName);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            return new ResourceFuncData(Encoding.ASCII.GetBytes(sb.ToString()));
        }

        public static ResourceFuncData StillAlive()
        {
            byte[] a = Encoding.ASCII.GetBytes("I feel FANTASTIC and I'm still alive");
            return new ResourceFuncData(a);
        }

        public static ResourceFuncData ProxyTest(string sdata)
        {
            Dictionary<string, string> data = Helpers.ParseAdditional(sdata);

            if (DatabaseHandler.FileCache.Download(new EHUri(new string[] { data["ipaddr"], data["port"], data["fileid"], data["keystamp"] }, EHUriType.ProxyTest)))
                return new ResourceFuncData(Encoding.ASCII.GetBytes(string.Format("{0}:OK-20", data["fileid"])));
            else
                return new ResourceFuncData(Encoding.ASCII.GetBytes(string.Format("{0}:FAIL-00", data["fileid"])));
        }

        public static ResourceFuncData SpeedTest(int size)
        {
            size = Math.Min(size, 1024 * 1024 * 256);

            byte[] buff = new byte[size];
            new Random().NextBytes(buff);
            return new ResourceFuncData(buff);
        }

        public static ResourceFuncData RefreshSettings()
        {
            return new ResourceFuncData(new byte[] { });
        }
    }
}
