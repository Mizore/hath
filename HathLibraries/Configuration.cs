using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using HathLibraries.DataTypes;
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace HathLibraries
{
    public static class Configuration
    {
        public static SCLocations Locations = new SCLocations();
        public static SCAccount Account = new SCAccount();
        public static SCConfig Config = new SCConfig();
        public static SCArgs Arguments = new SCArgs();
        public static IniManager IniManager = new IniManager();

#if DEBUG
        public static LogType MinimumLogLevel = LogType.Debug;
#else
        public static LogType MinimumLogLevel = LogType.Info;
#endif

        public static uint BufferSize = 1024 * 8; // 8 kb

        public static string ApiHost = "http://rpc.hentaiathome.net/clientapi.php";
        public static string ClientBuild = "66";

        public static ConsoleColor DefaultColor = ConsoleColor.Gray;
        public static CultureInfo Locale = new CultureInfo("en-US");

        public static Dictionary<string, ContentType> ImageContentTypeMap = new Dictionary<string, ContentType>()
        {
            { "jpg", ContentType.ImageJPG },
            { "jpeg", ContentType.ImageJPG },
            { "png", ContentType.ImagePNG },
            { "gif", ContentType.ImageGIF }
        };

        public static void Innitialize(string[] args)
        {
            Parsers.Parameters.Parse(args);

            Arguments.NoEHApi = Parsers.Parameters.Get<bool>("noapi");


            ThreadPool.SetMaxThreads(500, 250);
        }
    }

    public class SCArgs
    {
        public bool NoEHApi { get; set; }
    }

    public class SCLocations
    {
        public string Data = Path.GetDirectoryName(Application.ExecutablePath) + "/Data/";
        public string Temp = Path.GetDirectoryName(Application.ExecutablePath) + "/Data/Temp/";
        public string TempCache = Path.GetDirectoryName(Application.ExecutablePath) + "/Data/Temp/Cache/";
        public string Cache = Path.GetDirectoryName(Application.ExecutablePath) + "/Data/Cache/";
        public string Downloads = Path.GetDirectoryName(Application.ExecutablePath) + "/Data/Downloads/";
        public string Database = Path.GetDirectoryName(Application.ExecutablePath) + "/Data/Database.db";
        public string Log = Path.GetDirectoryName(Application.ExecutablePath) + "/Data/FullLog.txt";
        public string InfoFile = Path.GetDirectoryName(Application.ExecutablePath) + "/Data/Info.txt";
        public string IniFile = Path.GetDirectoryName(Application.ExecutablePath) + "/Data/Config.ini";
    }

    public class SCAccount
    {
        private int _deltaTime = 0;

        public string ClientID { get; set; }
        public string ClientKey { get; set; }
        public string ProxyPassKey { get; set; }
        public int CorrectedTime
        {
            get { return Helpers.CorrectedTime(this._deltaTime); }
            set { this._deltaTime = value; }
        }
    }

    public class SCConfig
    {
        public string[] RpcServers { get; set; }
        public string ImageServer { get; set; }
        public string Name { get; set; }
        public int Port { get; set; }

        public uint Throttle { get; set; }
        public ulong HourlyBytes { get; set; }
        public ulong DiskLimit { get; set; }
        public ulong DiskMinFree { get; set; }

        public string RequestServer { get; set; }
        public int RequestMode { get; set; }
    }
}
