using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HathLibraries
{
    public class IniManager
    {
        public string IniFile { get; set; }
        public Dictionary<string, string> Values = new Dictionary<string, string>();

        public IniManager()
        {
        }

        public void Start()
        {
            this.ParseFile();
        }

        private void ParseFile()
        {
            string[] Lines = File.ReadAllLines(Configuration.Locations.IniFile);
            foreach (string Line in Lines)
            {
                string Name = Line.Substring(0, Line.IndexOf("="));
                string Value = Line.Substring(Line.IndexOf("=") + 1);

                this.Values.Add(Name, Value);
            }
        }

        public void Set(string Name, string Value)
        {
            if (!this.Values.ContainsKey(Name))
                this.Values.Add(Name, Value);
            else
                this.Values[Name] = Value;

            this.Save();
        }

        public void SetDefaultRequired(string Name, string Value)
        {
            if (!this.Values.ContainsKey(Name))
                this.Values.Add(Name, Value);

            this.Save();
        }

        public void Save()
        {
            List<string> Lines = new List<string>();
            foreach (KeyValuePair<string, string> Pair in this.Values)
                Lines.Add(string.Format("{0}={1}", Pair.Key, Pair.Value));

            File.WriteAllLines(Configuration.Locations.IniFile, Lines.ToArray());
        }
    }
}
