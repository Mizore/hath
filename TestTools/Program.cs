using HathLibraries;
using HathLibraries.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestTools
{
    class Program
    {
        static void Main(string[] args)
        {
            int n = 0;

            Log.Start();

            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    n++;
                    int a = n;
                    Log.LogPosition Entry = Log.Add(LogType.Info, "{0} !~Red~SS", n);
                    Thread.Sleep(100);
                    Entry.Append(LogType.Info, " {0} !~Red~SS", a);
                }
            })).Start();

            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    n++;
                    int a = n;
                    Log.LogPosition Entry = Log.Add(LogType.Info, "{0} !~Green~DD", n);
                    Thread.Sleep(150);
                    Entry.Append(LogType.Info, " {0} !~Green~DD", a);
                }
            })).Start();

            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    n++;
                    int a = n;
                    Log.LogPosition Entry = Log.Add(LogType.Info, "{0} !~Yellow~AA", n);
                    Thread.Sleep(505);
                    Entry.Append(LogType.Info, " {0} !~Yellow~AA", a);
                }
            })).Start();

            Console.Read();
        }
    }
}
