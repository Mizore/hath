using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HathLibraries;

namespace HathServerGUI
{
    static class Program
    {
        static void Main(string[] args)
        {
            Application.Run(new ServerMain(args));
        }
    }

    public class ServerMain : ApplicationContext
    {
        public ServerMain(string[] args)
        {
            Configuration.Innitialize();

            ServerCore.PrepareFiles();
            ServerCore.LoadConfiguration();

            HathServer Server = ServerCore.InnitializeHathClient();
        }
    }
}
