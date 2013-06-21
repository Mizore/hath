using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Net.Sockets;

namespace HathLibraries.DataTypes
{
    public class UserHttpRequest
    {
        private static Dictionary<string, byte[]> ByteCache = new Dictionary<string, byte[]>();
        private static Dictionary<string, Type> FuncCache = new Dictionary<string, Type>();


        public byte[] SendBuffer { get; set; }
        public TcpClient Client { get; set; }
        public int BytesSent { get; set; }
        public ContentType ContentType { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public string ResourceToLoad { get; set; }
        public HttpResourceType ResourceType = HttpResourceType.None;
        public Dictionary<string, string> Headers;

        private ulong RequestID = 0;

        public delegate void DOnFinishUpload(ulong FullTime, ulong UploadTime, ulong Bytes, ulong RequestID);
        public event DOnFinishUpload EOnFinishUpload;

        private object[] Args;

        public UserHttpRequest(TcpClient Client, ulong ID)
        {
            this.Client = Client;
            this.RequestID = ID;
            this.Innitialize();
        }

        public void Innitialize()
        {
            this.SendBuffer = new byte[0];
            this.BytesSent = 0;
        }

        public void PrepareResource(HttpResourceType Type, string Name, params object[] Arguments)
        {
            this.ResourceType = Type;
            this.ResourceToLoad = Name;
            this.Args = Arguments;
        }

        public void LoadResource()
        {
            switch(this.ResourceType)
            {
                case HttpResourceType.File:
                    this.SendBuffer = File.ReadAllBytes(this.ResourceToLoad);
                    break;

                case HttpResourceType.Resource:
                    if (UserHttpRequest.ByteCache.ContainsKey(this.ResourceToLoad))
                        this.SendBuffer = UserHttpRequest.ByteCache[this.ResourceToLoad];
                    else
                        UserHttpRequest.ByteCache.Add(this.ResourceToLoad, this.SendBuffer = Content.Get<byte[]>(this.ResourceToLoad));

                    break;

                case HttpResourceType.Function:
                    string funcn = this.ResourceToLoad.Substring(this.ResourceToLoad.IndexOf('.') + 1);
                    object d = null;

                    if (UserHttpRequest.FuncCache.ContainsKey(this.ResourceToLoad))
                        d = UserHttpRequest.FuncCache[this.ResourceToLoad].InvokeMember(funcn, BindingFlags.Default | BindingFlags.InvokeMethod, null, null, this.Args);
                    else
                    {
                        string classn = string.Format("HathLibraries.DataTypes.{0}", this.ResourceToLoad.Substring(0, this.ResourceToLoad.IndexOf('.')));
                        Type t = Type.GetType(classn);
                        UserHttpRequest.FuncCache.Add(this.ResourceToLoad, t);
                        d = t.InvokeMember(funcn, BindingFlags.Default | BindingFlags.InvokeMethod, null, null, this.Args);
                    }

                    ResourceFuncData fdata = (ResourceFuncData)d;
                    this.SendBuffer = fdata.Data;
                    this.Headers = fdata.Headers;

                    if (fdata.ContentType != DataTypes.ContentType.Void)
                        this.ContentType = fdata.ContentType;


                    break;
            }

            if (this.SendBuffer == null || this.SendBuffer.Length == 0)
                this.StatusCode = HttpStatusCode.NotFount_404;
        }

        public void ThreadPoolCallback()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                Stopwatch sw1 = new Stopwatch();
                Stopwatch sw2 = new Stopwatch();
                sw1.Start();

                this.LoadResource();

                sb.AppendFormat("HTTP/1.0 {0}\r\n", this.StatusCode.GetStringValue());
                sb.AppendFormat("Connection: {0}\r\n", "close");

                if(this.Headers != null)
                    foreach (KeyValuePair<string, string> kvp in this.Headers)
                        sb.AppendFormat("{0}: {1}\r\n", kvp.Key, kvp.Value);


                if (this.StatusCode != HttpStatusCode.NotFount_404)
                {
                    sb.AppendFormat("Content-type: {0}\r\n", this.ContentType.GetStringValue());
                    sb.AppendFormat("Content-Length: {0}\r\n", this.SendBuffer.Length);
                    sb.Append("\r\n");

                    sw2.Start();

                    byte[] responseheader = Encoding.ASCII.GetBytes(sb.ToString());
                    this.Client.Client.Send(responseheader, 0, responseheader.Length, SocketFlags.None);

                    int PacketSize = 0;
                    while ((PacketSize = (int)Math.Min(Configuration.BufferSize, (this.SendBuffer.Length - this.BytesSent))) > 0)
                    {
                        this.Client.Client.Send(this.SendBuffer, this.BytesSent, PacketSize, SocketFlags.None);
                        this.BytesSent += PacketSize;
                        Stats.Trigger.ByteSent(PacketSize);
                    }

                    sw2.Stop();
                }
                else
                {
                    sb.Append("\r\n");
                    sw2.Start();
                    byte[] responseheader = Encoding.ASCII.GetBytes(sb.ToString());
                    this.Client.Client.Send(responseheader);
                    this.Client.Client.Send(Encoding.ASCII.GetBytes("You done goofd"));
                    sw2.Stop();
                }

                this.Client.Client.Close();

                sw1.Stop();

                if (this.ContentType == ContentType.Binary || this.ContentType == ContentType.ImageGIF || this.ContentType == ContentType.ImageJPG || this.ContentType == ContentType.ImagePNG)
                    Stats.Trigger.FileUpload(this.SendBuffer.Length);
                else
                    Stats.Trigger.OtherBytesSent(this.SendBuffer.Length);

                try { EOnFinishUpload((ulong)sw1.Elapsed.Milliseconds, (ulong)sw2.Elapsed.Milliseconds, (ulong)this.SendBuffer.Length, this.RequestID); }
                catch (Exception Ex)
                {
                    Ex.Print();
                }
            }
            catch (Exception Ex)
            {
                Ex.Print(LogType.Warning, false, false, this.RequestID);
            }

            GC.SuppressFinalize(this);
        }
    }
}
