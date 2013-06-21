using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HathLibraries;
using HathLibraries.DataTypes;

namespace HathServerConsole
{
    static class Program
    {
        static void Main(string[] args)
        {
            Log.Start();
            Configuration.Innitialize(args);

            ServerCore.PrepareFiles();
            ServerCore.LoadConfiguration();

            Console.Write(" ClientID: ");
            if (string.IsNullOrWhiteSpace(Configuration.Account.ClientID))
                Configuration.IniManager.Set("clientid", Console.ReadLine());
            //else
            //    Console.WriteLine(Configuration.IniManager.Values["clientid"]);

            Console.Write("ClientKey: ");
            if (string.IsNullOrWhiteSpace(Configuration.Account.ClientKey))
                Configuration.IniManager.Set("clientkey", Console.ReadLine());
            //else
            //    Console.WriteLine(Configuration.IniManager.Values["clientkey"]);


            ServerCore.InnitializeHathClient();
        }
    }
}
