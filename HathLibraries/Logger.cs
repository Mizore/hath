using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HathLibraries.DataTypes;
using System.Text.RegularExpressions;
using System.Threading;

namespace HathLibraries
{
    public static class Log
    {
        public class LogPosition
        {
            public int X { get; set; }
            public int Y { get; set; }

            public void Append(LogType Type, string Message, params object[] Args)
            {
                Log.WorkerData.Add(new LogWorkEntry(LogWorkType.Append, Type, this, Message, Args));
            }
        }

        private class LogWorkEntry
        {
            public LogWorkType WorkType { get; private set; }
            public LogType Type { get; private set; }
            public LogPosition Position { get; private set; }
            public string Message { get; private set; }
            public object[] Parameters { get; private set; }

            public LogWorkEntry(LogWorkType WorkType, LogType Type, LogPosition Pos, string Message, object[] Args)
            {
                this.WorkType = WorkType;
                this.Position = Pos;
                this.Type = Type;
                this.Message = Message;
                this.Parameters = Args;
            }
        }

        private enum LogWorkType
        {
            Add,
            AddEmpty,
            Append
        }

        public delegate void LEmpty();
        public delegate void LMess(string Message, params object[] Args);
        public delegate void LData(LogType Type, string Message, params object[] Args);

        public static event LEmpty EOnNewLine;
        public static event LMess EOnLogAppend;
        public static event LData EOnLog;

        private static int LastX = 0;
        private static int LastY = 0;

        private static object LogLock = new object();
        private static List<LogWorkEntry> WorkerData = new List<LogWorkEntry>();

        public static void Start()
        {
            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    if (LastY >= Console.BufferHeight - 1)
                    {
                        Console.Clear();
                        Console.SetCursorPosition(0, 0);

                        LastX = 0;
                        LastY = 0;
                    }

                    if (WorkerData.Count >= 1)
                    {
                        LogWorkEntry Entry = WorkerData.First();
                        if (Entry == null)
                            continue;

                        if (Entry.WorkType == LogWorkType.Add)
                        {
                            string Message = string.Format("\n{0} {1}", Entry.Type.GetStringValue(), string.Format(Entry.Message, Entry.Parameters));
                            Log.Print(Message, Entry.Type);

                            if (LastY < Console.CursorTop)
                            {
                                LastX = Console.CursorLeft;
                                LastY = Console.CursorTop;
                            }

                            Entry.Position.X = LastX;
                            Entry.Position.Y = LastY;

                            try { EOnLog(Entry.Type, Message, Entry.Parameters); }
                            catch { }
                        }else if (Entry.WorkType == LogWorkType.AddEmpty)
                        {
                            Log.Print("\n", Entry.Type);

                            if (LastY < Console.CursorTop)
                            {
                                LastX = Console.CursorLeft;
                                LastY = Console.CursorTop;
                            }

                            Entry.Position.X = LastX;
                            Entry.Position.Y = LastY;

                            try { EOnNewLine(); }
                            catch { }
                        }
                        else if (Entry.WorkType == LogWorkType.Append)
                        {
                            if (LastY >= Entry.Position.Y)
                            {

                                Console.SetCursorPosition(Entry.Position.X, Entry.Position.Y); // == Console.BufferHeight - 1 ? Entry.Position.Y + 1 : Entry.Position.Y

                                Log.Print(string.Format(Configuration.Locale, Entry.Message, Entry.Parameters), Entry.Type);

                                if (!Entry.Equals(WorkerData.Last()))
                                    Console.SetCursorPosition(Log.LastX, Log.LastY);

                                try { EOnLogAppend(Entry.Message, Entry.Parameters); }
                                catch { }
                            }
                        }

                        WorkerData.Remove(Entry);
                    }
                    else Thread.Sleep(10);
                }
            })).Start();
        }

        private static Dictionary<string, ConsoleColor> Coloring = new Dictionary<string, ConsoleColor>()
        {
            { "~White~", ConsoleColor.White },
            { "~Red~", ConsoleColor.Red },
            { "~Yellow~", ConsoleColor.Yellow },
            { "~Green~", ConsoleColor.Green },
            { "~Cyan~", ConsoleColor.Cyan },
            { "~Magenta~", ConsoleColor.Magenta },
            { "~Grey~", ConsoleColor.Gray },
            { "~Default~", Configuration.DefaultColor }
        };

        public static LogPosition Add(LogType Type, string Message, params object[] Args)
        {
            LogPosition LP = new LogPosition();
            WorkerData.Add(new LogWorkEntry(LogWorkType.Add, Type, LP, Message, Args));

            return LP;
        }

        public static LogPosition Add(LogType Type = LogType.Info)
        {
            if (Type < Configuration.MinimumLogLevel)
                return null;

            LogPosition LP = new LogPosition();
            WorkerData.Add(new LogWorkEntry(LogWorkType.AddEmpty, Type, LP, "\n", new object[] { }));

            return LP;
        }

        private static void Print(string Message, LogType Type = LogType.Info)
        {
            if (Type < Configuration.MinimumLogLevel)
                return;

            bool reset = false;

            string[] peaces = Regex.Split(Message, @"(\!?\~[A-Za-z]+\~|\s+)");
            foreach (string peace in peaces)
            {
                if (peace.StartsWith("!"))
                {
                    if (Coloring.ContainsKey(peace.Substring(1)))
                    {
                        Console.ForegroundColor = Coloring[peace.Substring(1)];
                        reset = true;
                    }
                }
                else
                {
                    if (Coloring.ContainsKey(peace))
                        Console.ForegroundColor = Coloring[peace];
                    else
                    {
                        Console.Write(peace);
                        if (reset)
                        {
                            reset = false;
                            Console.ForegroundColor = Configuration.DefaultColor;
                        }
                    }
                }
            }
        }
    }
}
