using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HathLibraries.DataTypes
{
    public class EHApiResponse
    {
        private string _head = "";
        private Dictionary<string, string> _params = new Dictionary<string,string>();

        private string data;

        public EHApiResponse(string data)
        {
            this.data = data;
            this.Parse();
        }

        private void Parse(bool FirstLineOnly = false)
        {
            string[] lines = this.data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            this._head = lines[0];

            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Contains("="))
                    this._params.Add(lines[i].Split('=')[0], lines[i].Split('=')[1]);
            }
        }

        public string Head
        {
            get { return this._head; }
        }

        public Dictionary<string, string> Data
        {
            get { return this._params; }
        }
    }
}
