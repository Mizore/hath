using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HathLibraries.DataTypes;

namespace HathLibraries
{
    public static class ServerCore
    {

        // 1st step
        public static void PrepareFiles()
        {
            if (!Directory.Exists(Configuration.Locations.Data))
                Directory.CreateDirectory(Configuration.Locations.Data);

            if (!Directory.Exists(Configuration.Locations.Cache))
                Directory.CreateDirectory(Configuration.Locations.Cache);

            if (!Directory.Exists(Configuration.Locations.Downloads))
                Directory.CreateDirectory(Configuration.Locations.Downloads);

            if (!Directory.Exists(Configuration.Locations.Temp))
                Directory.CreateDirectory(Configuration.Locations.Temp);

            if (!Directory.Exists(Configuration.Locations.TempCache))
                Directory.CreateDirectory(Configuration.Locations.TempCache);

            if (!File.Exists(Configuration.Locations.Database))
                File.Create(Configuration.Locations.Database).Close();

            //    using (BinaryWriter W = new BinaryWriter(File.Open(Configuration.Locations.Database, FileMode.Create)))
            //        W.Write(global::Hath.Properties.Resources.Database);

            //if (!File.Exists(Configuration.Locations.InfoFile))
            //    using (BinaryWriter W = new BinaryWriter(File.Open(Configuration.Locations.InfoFile, FileMode.Create)))
            //        W.Write(global::Hath.Properties.Resources.infoFile);

            if (!File.Exists(Configuration.Locations.Log))
                File.Create(Configuration.Locations.Log).Close();

            if (!File.Exists(Configuration.Locations.IniFile))
                File.Create(Configuration.Locations.IniFile).Close();

            DatabaseHandler.Innitialize();
            Stats.Start();
        }

        // 2nd step
        public static void LoadConfiguration()
        {
            Configuration.IniManager.Start();
            Configuration.IniManager.SetDefaultRequired("allow_stats", "true");
            Configuration.IniManager.SetDefaultRequired("buffer_size", "8192");
            Configuration.IniManager.SetDefaultRequired("clientid", "");
            Configuration.IniManager.SetDefaultRequired("clientkey", "");

            Configuration.Account.ClientID = Configuration.IniManager.Values["clientid"];
            Configuration.Account.ClientKey = Configuration.IniManager.Values["clientkey"];
        }

        // 3rd step
        public static HathServer InnitializeHathClient()
        {
            Log.LogPosition LogEntry;

            LogEntry = Log.Add(LogType.Info, "Receiving server stat... ");
            try
            {
                EHApiResponse stat = EHApi.ServerStat();
                Configuration.Account.CorrectedTime = int.Parse(stat.Data["server_time"]) - Helpers.UnixTime();
            }
            catch(Exception Ex)
            {
                LogEntry.Append(LogType.Info, "!~Red~Failed");
                Ex.Print();
                return null;
            }
            LogEntry.Append(LogType.Info, "!~Green~Done");

            LogEntry = Log.Add(LogType.Info, "Receiving client configuration... ");
            try
            {
                EHApiResponse conf = EHApi.ClientLogin();
                if (conf.Head != "OK")
                {
                    LogEntry.Append(LogType.Info, "!~Yellow~{0}", conf.Head);
                    return null;
                }

                Configuration.Config.RpcServers = conf.Data["rpc_server_ip"].Split(';');
                Configuration.Config.ImageServer = conf.Data["image_server"];
                Configuration.Config.Name = conf.Data["name"];
                Configuration.Config.Port = int.Parse(conf.Data["port"]);
                Configuration.Config.Throttle = uint.Parse(conf.Data["throttle_bytes"]);
                Configuration.Config.HourlyBytes = ulong.Parse(conf.Data["hourbwlimit_bytes"]);
                Configuration.Config.DiskLimit = ulong.Parse(conf.Data["disklimit_bytes"]);
                Configuration.Config.DiskMinFree = ulong.Parse(conf.Data["diskremaining_bytes"]);
                Configuration.Config.RequestServer = conf.Data["request_server"];
                Configuration.Config.RequestMode = int.Parse(conf.Data["request_proxy_mode"]);
            }
            catch (Exception Ex)
            {
                LogEntry.Append(LogType.Info, "!~Red~Failed");
                Ex.Print();
                return null;
            }
            LogEntry.Append(LogType.Info, "!~Green~Done");

            LogEntry = Log.Add(LogType.Info, "Passing account configuration to server instance... ");
            HathServer server = new HathServer();
            LogEntry.Append(LogType.Info, "!~Green~Done");

            server.Start();
            return server;
        }
    }
}
