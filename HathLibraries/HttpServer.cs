using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using HathLibraries.DataTypes;
using System.Net.Sockets;

namespace HathLibraries
{
    public class HttpServer
    {
        private TcpListener Listener;
        private Thread ConnectionListenerWorker;

        private ulong RequestID = 0;

        public HttpServer()
        {
        }

        public void Start(int port)
        {
            this.Listener = new TcpListener(IPAddress.Any, port);
            this.Listener.Start();

            this.ConnectionListenerWorker = new Thread(new ThreadStart(this.ConnectionListener));
            this.ConnectionListenerWorker.Start();
        }

        private void ConnectionListener()
        {
            while(true)
            {
                TcpClient Client;
                try
                {
                    Client = Listener.AcceptTcpClient();
                }
                catch (SocketException)
                {
                    break;
                }
                catch (InvalidOperationException)
                {
                    break;
                }
                if (Client.Connected)
                {
                    Client.ReceiveTimeout = 60000;
                    Client.SendTimeout = 60000;

                    ThreadPool.QueueUserWorkItem(HandleClient, Client);
                }
            }
        }

        private void HandleClient(object oClient)
        {
            TcpClient Client = (TcpClient)oClient;
            ulong cRequest = ++RequestID;

            Stats.Trigger.HttpRequestReceived();

            HttpRequestData RData = Helpers.GetRequestData(Client);
            UserHttpRequest Request = new UserHttpRequest(Client, cRequest);

            if (RData.RequestUri == null)
            {
                // quick fix, needs propper fix
                return;
            }

            string[] UrlPeaces = RData.RequestUri.Substring(1).Split('/');
            switch (RData.RequestUri)
            {
                case "/":
                    Request.ContentType = ContentType.TextHtml;
                    Request.PrepareResource(HttpResourceType.Resource, "Http.Index");
                    break;

                case "/stats/json":
                    Request.ContentType = ContentType.TextPlain;
                    Request.PrepareResource(HttpResourceType.Function, "HttpLiveResponses.LiveStatsJson");
                    break;

                case "/stats/caps":
                    Request.ContentType = ContentType.TextPlain;
                    Request.PrepareResource(HttpResourceType.Function, "HttpLiveResponses.LiveStatsCap");
                    break;

                case "/data/css/bootstrap.min":
                    Request.ContentType = ContentType.TextCss;
                    Request.PrepareResource(HttpResourceType.Resource, "Http.Css.Bootstrap");
                    break;

                case "/data/css/bootstrap.responsive.min":
                    Request.ContentType = ContentType.TextCss;
                    Request.PrepareResource(HttpResourceType.Resource, "Http.Css.Bootstrap.Responsive");
                    break;

                case "/data/css/custom":
                    Request.ContentType = ContentType.TextCss;
                    Request.PrepareResource(HttpResourceType.Resource, "Http.Css.Custom");
                    break;

                case "/data/javascript/jquery.min":
                    Request.ContentType = ContentType.TextJavascript;
                    Request.PrepareResource(HttpResourceType.Resource, "Http.Javascript.jQuery");
                    break;

                case "/data/javascript/bootstrap.min":
                    Request.ContentType = ContentType.TextJavascript;
                    Request.PrepareResource(HttpResourceType.Resource, "Http.Javascript.Bootstrap");
                    break;

                case "/data/javascript/custom":
                    Request.ContentType = ContentType.TextJavascript;
                    Request.PrepareResource(HttpResourceType.Resource, "Http.Javascript.Custom");
                    break;


                case "/favicon.ico":
                    Request.ContentType = ContentType.ImageIcon;
                    Request.PrepareResource(HttpResourceType.Resource, "Icons.Favicon");
                    break;

                case "/robots.txt":
                    Request.ContentType = ContentType.TextPlain;
                    Request.PrepareResource(HttpResourceType.Resource, "Text.Robots");
                    break;

                default:
                    switch (UrlPeaces[0])
                    {
                        case "h":
                            Request.PrepareResource(HttpResourceType.Function, "HttpFileServe.Download", UrlPeaces);
                            break;

                        // this requiest is no longer used at all?

                        //case "r":
                        //    Request.PrepareResource(HttpResourceType.Function, "HttpFileServe.R", UrlPeaces);
                        //    break;

                        case "p":
                            Request.PrepareResource(HttpResourceType.Function, "HttpFileServe.Proxy", UrlPeaces);
                            break;

                        case "servercmd":
                            if (UrlPeaces.Length < 5)
                            {

                            }
                            else
                            {
                                Dictionary<string, string> apar = Helpers.ParseAdditional(UrlPeaces[2]);

                                switch (UrlPeaces[1])
                                {
                                    case "still_alive":
                                        Request.ContentType = ContentType.TextPlain;
                                        Request.PrepareResource(HttpResourceType.Function, "HttpServerCmd.StillAlive");
                                        break;

                                    case "speed_test":
                                        Request.ContentType = ContentType.Binary;
                                        Request.PrepareResource(HttpResourceType.Function, "HttpServerCmd.SpeedTest", int.Parse(apar["testsize"]));
                                        break;

                                    case "cache_list":
                                        Request.ContentType = ContentType.TextPlain;
                                        Request.PrepareResource(HttpResourceType.Function, "HttpServerCmd.CacheList", int.Parse(apar["max_filesize"]), int.Parse(apar["max_filecount"]));
                                        break;

                                    case "cache_files":
                                        Request.ContentType = ContentType.TextPlain;
                                        Request.PrepareResource(HttpResourceType.Function, "HttpServerCmd.CacheFiles", UrlPeaces[2]);
                                        break;
                                }
                            }
                            break;
                    }
                    break;
            }

            Request.EOnFinishUpload += (ulong FullTime, ulong UploadTime, ulong Bytes, ulong Rid) =>
            {
                Log.Add(LogType.Http, "{0,13:N0} FIN > Total: {1,-9:#,0 ms}   Upload: {2,-9:#,0 ms}   Size: {3,-16:#,0 bytes}   Speed: {4,-11:#,0 kb/s}", Rid, FullTime, UploadTime, Bytes, Math.Round((double)Bytes / (double)UploadTime));
            };

            Log.Add(LogType.Http, "{0,13:N0} OUT > {1}", cRequest, RData.RequestUri);
            Request.ThreadPoolCallback();
        }
    }
}
