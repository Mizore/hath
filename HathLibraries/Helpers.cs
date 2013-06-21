using HathLibraries.DataTypes;
using HttpMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace HathLibraries
{
    public static class Helpers
    {
        private static DateTime UnixDT = new DateTime(1970, 1, 1, 0, 0, 0);

        public static int CorrectedTime(int tOffset)
        {
            return UnixTime() + tOffset;
        }

        public static int UnixTime()
        {
            return (int)(DateTime.UtcNow - UnixDT).TotalSeconds;
        }

        public static Dictionary<string, string> ParseAdditional(string add)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(add))
            {
                string[] peaces = add.Split(';');
                foreach (string peace in peaces)
                {
                    string[] kvp = peace.Split('=');

                    data.Add(kvp[0], kvp[1]);
                }
            }

            return data;
        }

        public static HttpRequestData GetRequestData(TcpClient Client)
        {
            byte[] requestbuff = new byte[1024 * 32];
            int received = 0;

            try
            {
                received = Client.Client.Receive(requestbuff, requestbuff.Length, 0);
            }
            catch { }

            Stats.Trigger.ByteReceived(received);

            byte[] rbuf = new byte[received];
            Array.Copy(requestbuff, rbuf, received);

            Stats.Trigger.OtherBytesReceived(received);

            HttpRequestData data = new HttpRequestData();
            HttpParser parser = new HttpParser(data);

            parser.Execute(new ArraySegment<byte>(rbuf, 0, rbuf.Length));

            return data;
        }
    }
}
