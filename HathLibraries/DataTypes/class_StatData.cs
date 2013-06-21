using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HathLibraries.DataTypes
{
    public class StatData
    {
        #region Life-time stats

            public ulong TotalBytesSent { get; set; }
            public ulong TotalBytesReceived { get; set; }

            public ulong TotalFilesUploaded { get; set; }
            public ulong TotalFilesDownloaded { get; set; }

            public uint TotalHttpRequestsRevceived { get; set; }
            public uint TotalHttpRequestsSent { get; set; }

        #endregion

        #region Current session stats

            public uint FilesDownloadedThisRun { get; set; }
            public uint FilesUploadedThisRun { get; set; }

            public ulong BytesSentThisRun { get; set; }
            public ulong BytesReceivedThisRun { get; set; }

            public uint HttpRequestsReceivedThisRun { get; set; }
            public uint HttpRequestsSentThisRun { get; set; }

            public uint NewFilesThisRun { get; set; }

        #endregion

        #region Delta stats ( reset every second )

            public uint BytesSentDelta { get; set; }
            public uint BytesReceivedDelta { get; set; }
            public uint FilesSentDelta { get; set; }
            public uint FilesReceivedDelta { get; set; }

            public uint OveralUploadDelta { get; set; }
            public uint OveralDownloadDelta { get; set; }

        #endregion

        #region Misc stats

            public uint FilesInCache { get; set; }
            public ulong UsedSpace { get; set; }

            public uint UnregisteredFiles { get; set; }

            public ushort LastKeepAliveSecondsAgo { get; set; }

            public double RequestsPerSecond { get; set; }

        #endregion
    }
}
