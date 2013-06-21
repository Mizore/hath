using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

namespace HathLibraries.DataTypes
{
    public static class HttpLiveResponses
    {
        private static string aLib;
        private static string aConsole;
        private static string aGui;

        private static void LoadVersions()
        {
            if (aLib == null || aConsole == null || aGui == null)
            {
                try
                {
                    string aLibDir = Assembly.GetExecutingAssembly().Location;
                    string aLibVer = FileVersionInfo.GetVersionInfo(aLibDir).FileVersion.ToString();
                    DateTime aLibTime = new FileInfo(aLibDir).LastWriteTime;
                    aLib = string.Format("{0,-14} {1:yyyy/MM/dd HH:mm:ss}", aLibVer, aLibTime);
                }
                catch { aLib = "???"; }

                try
                {
                    string aConsoleDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\HathServerConsole.exe";
                    string aConsoleVer = FileVersionInfo.GetVersionInfo(aConsoleDir).FileVersion.ToString();
                    DateTime aConsoleTime = new FileInfo(aConsoleDir).LastWriteTime;
                    aConsole = string.Format("{0,-14} {1:yyyy/MM/dd HH:mm:ss}", aConsoleVer, aConsoleTime);
                }
                catch { aConsole = "???"; }

                try
                {
                    string aGuiDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\HathServerGUI.exe";
                    string aGuiVer = FileVersionInfo.GetVersionInfo(aGuiDir).FileVersion.ToString();
                    DateTime aGuiTime = new FileInfo(aGuiDir).LastWriteTime;
                    aGui = string.Format("{0,-14} {1:yyyy/MM/dd HH:mm:ss}", aGuiVer, aGuiTime);
                }
                catch { aGui = "???"; }
            }
        }

        public static ResourceFuncData LiveStatsJson()
        {
            LoadVersions();

            string data = new JavaScriptSerializer().Serialize(Stats.Data);

            return new ResourceFuncData(Encoding.ASCII.GetBytes(data), ContentType.TextPlain);
        }

        public static ResourceFuncData LiveStatsCap()
        {
            string data = new JavaScriptSerializer().Serialize(new Dictionary<string, uint>()
            {
                { "UploadMax", Configuration.Config.Throttle },
                { "DownloadMax", Configuration.Config.Throttle }
            });

            return new ResourceFuncData(Encoding.ASCII.GetBytes(data), ContentType.TextPlain);
        }

        public static ResourceFuncData LiveStats()
        {
            LoadVersions();

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Configuration.Locale,"                                                                                                                                                                        \n");
            sb.AppendFormat(Configuration.Locale, "                                                                                                                                                                        \n");
            sb.AppendFormat(Configuration.Locale, "                                                                                                                                                                        \n");
            sb.AppendFormat(Configuration.Locale, "          +------------------------------------------------------------------------------------------------------------------------------------------------------------+\n");
            sb.AppendFormat(Configuration.Locale, "          |                                                                                                                                                            |\n");
            sb.AppendFormat(Configuration.Locale, "          |      Hentai@Home client v2.0                                                                                                                               |\n");
            sb.AppendFormat(Configuration.Locale, "          |                                                                                                                                                            |\n");
            sb.AppendFormat(Configuration.Locale, "          +------------------------------------------------------------------------------------------------------------------------------------------------------------+\n");
            sb.AppendFormat(Configuration.Locale, "          | Versions                                                                                                                                                   |\n");
            sb.AppendFormat(Configuration.Locale, "          |                                                                                                                                                            |\n");
            sb.AppendFormat(Configuration.Locale, "          |             Hath libraries: {0}                                                                                             |\n", aLib);
            sb.AppendFormat(Configuration.Locale, "          |      Hath server (console): {0}                                                                                             |\n", aConsole);
            sb.AppendFormat(Configuration.Locale, "          |      Hath server (gui)    : {0}                                                                                             |\n", aGui);
            sb.AppendFormat(Configuration.Locale, "          +------------------------------------------------------------------------------------------------------------------------------------------------------------+\n");
            sb.AppendFormat(Configuration.Locale, "          | Source code                                                                                                                                                |\n");
            sb.AppendFormat(Configuration.Locale, "          |                                                                                                                                                            |\n");
            sb.AppendFormat(Configuration.Locale, "          |      Public repository soon...                                                                                                                             |\n");
            sb.AppendFormat(Configuration.Locale, "          +------------------------------------------------------------------------------------------------------------------------------------------------------------+\n");
            sb.AppendFormat(Configuration.Locale, "          | Project team                                                                                                                                               |\n");
            sb.AppendFormat(Configuration.Locale, "          |                                                                                                                                                            |\n");
            sb.AppendFormat(Configuration.Locale, "          |      Miz           ( MizChan )                        miz@hexide.com                                                                                       |\n");
            sb.AppendFormat(Configuration.Locale, "          |      Dalek Caan    ( Katz )                           coladict@gmail.com                                                                                   |\n");
            sb.AppendFormat(Configuration.Locale, "          |      Dark          ( Darkimmortal )                   admin@imgkk.com                                                                                      |\n");
            sb.AppendFormat(Configuration.Locale, "          +------------------------------------------------------------------------------------------------------------------------------------------------------------+\n");
            sb.AppendFormat(Configuration.Locale, "          | Server runtime statistics                                                                                                                                  |\n");
            sb.AppendFormat(Configuration.Locale, "          |                                                                                                                                                            |\n");
            sb.AppendFormat(Configuration.Locale, "          |                   Current run                                     All time                                   Other                                         |\n");
            sb.AppendFormat(Configuration.Locale, "          |                                                                                                                                                            |\n");
            sb.AppendFormat(Configuration.Locale, "          |        Http requests received: {0,-12:N0}         Http requests received: {1,-15:N0}         Files in cache: {2,-13:N0}                          |\n", Stats.Data.HttpRequestsReceivedThisRun, Stats.Data.TotalHttpRequestsRevceived, Stats.Data.FilesInCache);
            sb.AppendFormat(Configuration.Locale, "          |            Http requests sent: {0,-12:N0}             Http requests sent: {1,-15:N0}             Space used: {2,-32:#,0 bytes}       |\n", Stats.Data.HttpRequestsSentThisRun, Stats.Data.TotalHttpRequestsSent, Stats.Data.UsedSpace);
            sb.AppendFormat(Configuration.Locale, "          |                                                                                                 Unregistered files: {0,-13:N0}                          |\n", Stats.Data.UnregisteredFiles);
            sb.AppendFormat(Configuration.Locale, "          |                Files received: {0,-12:N0}                 Files received: {1,-15:N0}        Last keep alive: {2,-13:N0}                          |\n", Stats.Data.FilesDownloadedThisRun, Stats.Data.TotalFilesDownloaded, Stats.Data.LastKeepAliveSecondsAgo);
            sb.AppendFormat(Configuration.Locale, "          |                    Files sent: {0,-12:N0}                     Files sent: {1,-15:N0}    Requests per second: {2,-8:N2}                               |\n", Stats.Data.FilesUploadedThisRun, Stats.Data.TotalFilesUploaded, Stats.Data.RequestsPerSecond);
            sb.AppendFormat(Configuration.Locale, "          |                                                                                                                                                            |\n");
            sb.AppendFormat(Configuration.Locale, "          |                Bytes received: {0,-18:#,0 bytes}           Bytes received: {1,-32:#,0 bytes}                                               |\n", Stats.Data.BytesReceivedThisRun, Stats.Data.TotalBytesReceived);
            sb.AppendFormat(Configuration.Locale, "          |                    Bytes sent: {0,-18:#,0 bytes}               Bytes sent: {1,-32:#,0 bytes}                                               |\n", Stats.Data.BytesSentThisRun, Stats.Data.TotalBytesSent);
            sb.AppendFormat(Configuration.Locale, "          |                                                                                                                                                            |\n");
            sb.AppendFormat(Configuration.Locale, "          |   Upload speed: >|{0,-119}|< {1,9:N} kb/s |\n", new string('=', (int)Math.Min(119, ((double)Stats.Data.BytesSentDelta / (double)Configuration.Config.Throttle) * 119)), Stats.Data.BytesSentDelta / 1024.0);
            sb.AppendFormat(Configuration.Locale, "          | Download speed: >|{0,-119}|< {1,9:N} kb/s |\n", new string('=', (int)Math.Min(119, ((double)Stats.Data.BytesReceivedDelta / (double)Configuration.Config.Throttle) * 119)), Stats.Data.BytesReceivedDelta / 1024.0);
            sb.AppendFormat(Configuration.Locale, "          +------------------------------------------------------------------------------------------------------------------------------------------------------------+\n");
            sb.AppendFormat(Configuration.Locale, "                                                                                                                                                                        \n");
            sb.AppendFormat("                                                                                                                                                                        \n");


            return new ResourceFuncData(
                Encoding.ASCII.GetBytes(sb.ToString()),
                ContentType.TextPlain,
                new Dictionary<string, string>()
                {
                    { "Refresh", "1" }
                });
        }
    }
}
