using HathLibraries.DataTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace HathLibraries
{
    public static class Stats
    {
        private static uint TempHttpRequestsRevceived = 0;
        private static uint TempHttpRequestsSent = 0;

        private static ulong TotalBytesSent = 0;
        private static ulong TotalBytesReceived = 0;
        private static ulong TotalFilesUploaded = 0;
        private static ulong TotalFilesDownloaded = 0;
        private static uint TotalHttpRequestsRevceived = 0;
        private static uint TotalHttpRequestsSent = 0;


        private static ulong BytesSentThisRun = 0;
        private static ulong BytesReceivedThisRun = 0;
        private static uint FilesDownloadedThisRun = 0;
        private static uint FilesUploadedThisRun = 0;
        private static uint HttpRequestsReceivedThisRun = 0;
        private static uint HttpRequestsSentThisRun = 0;
        private static uint NewFilesThisRun = 0;


        private static ulong UsedSpace = 0;
        private static uint FilesInCache = 0;
        private static uint UnregisteredFiles = 0;
        private static ushort LastKeepAliveSecondsAgo = 0;


        private static uint BytesSentDelta = 0;
        private static uint BytesReceivedDelta = 0;
        private static uint FilesSentDelta = 0;
        private static uint FilesReceivedDelta = 0;

        private static double RequestsPerSecond = 0.0;

        private static Timer tTimer;
        private static StatData SData;

        #region Event stuff

        public delegate void ENone();
        public delegate void EInt(uint count);
        public delegate void EDInt(uint c1, uint c2);
        public delegate void ELong(ulong count);
        public delegate void EDLong(ulong c1, ulong c2);
        public delegate void EShort(ushort count);
        public delegate void EDouble(double count);

        public static event EDLong FileDownloaded;
        public static event EDLong FileUploaded;

        #endregion


        private static PerformanceCounterCategory ONetworkCat = new PerformanceCounterCategory("Network Interface");
        private static string ONetworkName;

        private static PerformanceCounter ONetworkSent;
        private static PerformanceCounter ONetworkReceived;


        public static void Start()
        {
            StatData Init = DatabaseHandler.StatCache.GetInnitial();

            ONetworkName = ONetworkCat.GetInstanceNames()[0];

            uint tlmax = 0;
            string[] a = ONetworkCat.GetInstanceNames();

            foreach (string asd in ONetworkCat.GetInstanceNames())
            {
                PerformanceCounter tmp = new PerformanceCounter("Network Interface", "Bytes Sent/sec", asd);
                if (tmp.RawValue > 0 && tmp.RawValue > tlmax)
                {
                    ONetworkSent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", asd);
                    ONetworkReceived = new PerformanceCounter("Network Interface", "Bytes Received/sec", asd);
                }
            }

            TotalBytesSent = Init.TotalBytesSent;
            TotalBytesReceived = Init.TotalBytesReceived;
            TotalFilesUploaded = Init.TotalFilesUploaded;
            TotalFilesDownloaded = Init.TotalFilesDownloaded;
            TotalHttpRequestsRevceived = Init.TotalHttpRequestsRevceived;
            TotalHttpRequestsSent = Init.TotalHttpRequestsSent;

            tTimer = new Timer(new TimerCallback((object TimerState) =>
            {
                SData = new StatData()
                {
                    TotalBytesSent = TotalBytesSent,
                    TotalBytesReceived = TotalBytesReceived,
                    TotalFilesUploaded = TotalFilesUploaded,
                    TotalFilesDownloaded = TotalFilesDownloaded,
                    TotalHttpRequestsRevceived = TotalHttpRequestsRevceived,
                    TotalHttpRequestsSent = TotalHttpRequestsSent,
                    BytesSentThisRun = BytesSentThisRun,
                    BytesReceivedThisRun = BytesReceivedThisRun,
                    FilesDownloadedThisRun = FilesDownloadedThisRun,
                    FilesUploadedThisRun = FilesUploadedThisRun,
                    HttpRequestsReceivedThisRun = HttpRequestsReceivedThisRun,
                    HttpRequestsSentThisRun = HttpRequestsSentThisRun,
                    NewFilesThisRun = NewFilesThisRun,
                    UsedSpace = UsedSpace,
                    FilesInCache = FilesInCache,
                    UnregisteredFiles = UnregisteredFiles,
                    LastKeepAliveSecondsAgo = LastKeepAliveSecondsAgo,

                    BytesSentDelta = BytesSentDelta,
                    BytesReceivedDelta = BytesReceivedDelta,
                    FilesSentDelta = FilesSentDelta,
                    FilesReceivedDelta = FilesReceivedDelta,

                    RequestsPerSecond = RequestsPerSecond,

                    OveralUploadDelta = (uint)ONetworkSent.NextValue(),
                    OveralDownloadDelta = (uint)ONetworkReceived.NextValue()
                };

                DatabaseHandler.StatCache.Update(BytesSentDelta, BytesReceivedDelta, TempHttpRequestsRevceived, TempHttpRequestsSent, FilesSentDelta, FilesReceivedDelta);

                BytesSentDelta = 0;
                BytesReceivedDelta = 0;
                FilesSentDelta = 0;
                FilesReceivedDelta = 0;

                TempHttpRequestsRevceived = 0;
                TempHttpRequestsSent = 0;

                RequestsPerSecond = 0;
            }), null, 0, 1000);
        }

        public static StatData Data { get { return SData; } }

        public static class Trigger
        {
            public static void PendingFiles(int count)
            {
                UnregisteredFiles = (uint)count;
            }
            public static void KeepAlive(int Last)
            {
                try { LastKeepAliveSecondsAgo = (ushort)Last; }
                catch { }
            }
            public static void FileDownload(int FileSize)
            {
                FilesDownloadedThisRun += 1;
                FilesReceivedDelta += 1;
                TotalFilesDownloaded += 1;

                TotalBytesReceived += (uint)FileSize;
                BytesReceivedThisRun += (uint)FileSize;
                UsedSpace += (ulong)FileSize;

                try { FileDownloaded(FilesDownloadedThisRun, TotalFilesDownloaded); }
                catch { }
            }
            public static void FileUpload(int FileSize)
            {
                FilesUploadedThisRun += 1;
                TotalFilesUploaded += 1;
                FilesSentDelta += 1;

                TotalBytesSent += (uint)FileSize;
                BytesSentThisRun += (uint)FileSize;

                try { FileUploaded(FilesUploadedThisRun, TotalFilesUploaded); }
                catch { }
            }
            public static void FileCache(int FileSize, bool AffectStats = true)
            {
                FilesInCache += 1;
                UsedSpace += (ulong)FileSize;

                if (AffectStats)
                {
                    TotalFilesDownloaded += 1;
                    FilesDownloadedThisRun += 1;
                    TotalBytesReceived += (uint)FileSize;
                }
            }

            public static void OtherBytesSent(int Amount)
            {
                TotalBytesSent += (uint)Amount;
                BytesSentThisRun += (uint)Amount;
            }
            public static void OtherBytesReceived(int Amount)
            {
                TotalBytesReceived += (uint)Amount;
                BytesReceivedThisRun += (uint)Amount;
            }

            public static void HttpRequestReceived()
            {
                TempHttpRequestsRevceived += 1;
                HttpRequestsReceivedThisRun += 1;
                TotalHttpRequestsRevceived += 1;

                RequestsPerSecond += 1;
            }
            public static void HttpRequestSent()
            {
                TempHttpRequestsSent += 1;
                HttpRequestsSentThisRun += 1;
                TotalHttpRequestsSent += 1;
            }

            public static void ByteSent(int Amount)
            {
                BytesSentDelta += (uint)Amount;
            }
            public static void ByteReceived(int Amount)
            {
                BytesReceivedDelta += (uint)Amount;
            }
        }
    }
}
