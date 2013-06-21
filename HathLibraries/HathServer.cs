//#define DEBUG

using HathLibraries.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HathLibraries
{
    public class HathServer
    {
        private HttpServer HttpServer;
        private Timer KeepAliveTimer;
        private int KeepAliveTicks = 0;

        public HathServer()
        {
        }

        public void Stop()
        {
            Log.LogPosition LogEntry = Log.Add(LogType.Info, "Closing database... ");
            if (DatabaseHandler.Close())
                LogEntry.Append(LogType.Info, "~!Green~Success");
            else
                LogEntry.Append(LogType.Error, "~!Red~Fail");

            if (!Configuration.Arguments.NoEHApi)
            {
                EHApiResponse stop = EHApi.ClientStop();
                LogEntry = Log.Add(LogType.Info, "Notifying EHApi with 'client_stop'... ");
                if (stop.Head == "OK")
                    LogEntry.Append(LogType.Info, "~!Green~Success");
                else
                {
                    Log.Add(LogType.Error, "!~Red~Failed to properly close server session.");
                    Log.Add(LogType.Debug, "!~Red~Head: {0}; Data: {1}", stop.Head, stop.Data);
                    LogEntry = Log.Add(LogType.Info, "Notifying EHApi with 'client_stop'... ");
                    if (EHApi.ClientStop().Head == "OK")
                        LogEntry.Append(LogType.Info, "~!Green~Success");
                    else
                        LogEntry.Append(LogType.Info, "~!Red~Fail");
                }
            }
        }

        public void Start()
        {
            Log.Add();
            Log.Add(LogType.Info, "Beginning start up process:");

            DatabaseHandler.FileCache.Verify();

            Log.LogPosition LogEntry;

            LogEntry = Log.Add(LogType.Info, "    Starting http server... ");
            try
            {
                this.HttpServer = new HttpServer();
                this.HttpServer.Start(Configuration.Config.Port);

                LogEntry.Append(LogType.Info, "Listening on port !~Green~{0}", Configuration.Config.Port);
            }
            catch (Exception Ex)
            {
                LogEntry.Append(LogType.Info, "!~Red~Failed");
                Ex.Print();
                return;
            }

            if (!Configuration.Arguments.NoEHApi)
            {
                EHApiResponse start = EHApi.ClientStart();
                if (start.Head.StartsWith("FAIL_CONNECT_TEST"))
                {
                    Log.Add(LogType.Error, "        Failed server port check...");
                    return;
                }
                else if (start.Head.StartsWith("FAIL_STARTUP_FLOOD"))
                {
                    Log.Add(LogType.Error, "        Startup flood");
                    return;
                }
                else if (start.Head.StartsWith("FAIL_OTHER_CLIENT_CONNECTED"))
                {
                    Log.Add(LogType.Error, "        Other client connected");
                    return;
                }
                else if (start.Head == "OK")
                {
                    Log.Add(LogType.Info, "        ~Green~Client has started.");
                    Log.Add();
                    // Finish up startup, hook up all events and timers 
                    // todo

                    this.KeepAliveTimer = new Timer(new TimerCallback((object state) =>
                    {
                        Stats.Trigger.KeepAlive(this.KeepAliveTicks);
                        if (++this.KeepAliveTicks < 30)
                            return;

                        LogEntry = Log.Add(LogType.Info, "Requesting keep alive... ");
                        try
                        {
                            this.KeepAliveTicks = 0;
                            EHApiResponse stillalive = EHApi.StillAlive();
                            LogEntry.Append(LogType.Info, "!~Green~Ok");
                        }
                        catch (Exception Ex)
                        {
                            LogEntry.Append(LogType.Info, "!~Red~Failed");
                            Ex.Print();
                        }
                    }), null, 5000, 1000);
                }
                else
                {
                    Log.Add(LogType.Error, "        {0}", start.Head);
                    return;
                }
            }
        }
    }
}
