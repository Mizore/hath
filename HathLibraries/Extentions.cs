using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using HathLibraries.DataTypes;
using System.Security.Cryptography;
using System.Net;
using System.Web;

namespace HathLibraries
{
    public static class Extentions
    {
        public static string GetStringValue(this Enum value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            string description = value.ToString();
            FieldInfo fieldInfo = value.GetType().GetField(description);
            StringAttribute[] attributes = (StringAttribute[])fieldInfo.GetCustomAttributes(typeof(StringAttribute), false);

            if (attributes != null && attributes.Length > 0)
                description = attributes[0].Type;

            return description;
        }

        public static void Print(this Exception e, LogType Level = LogType.Error, bool PrintStack = true, bool Padding = true, ulong conid = 0)
        {
            if (Padding)
                Log.Add();

            Log.Add(Level, "{0}{1}{2}", conid > 0 ? string.Format("{0,13:N0} ", conid) : "", PrintStack ? "Error: " : "", e.Message);

            if (PrintStack)
            {
                Log.Add(Level, "{0}Stack:", conid > 0 ? string.Format("{0,13:N0} ", conid) : "");
                e.StackTrace.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(line => Log.Add(Level, "{0}{1}", conid > 0 ? string.Format("{0:13:N0} ", conid) : "", line));
            }

            if (Padding)
                Log.Add();
        }

        public static string HashSHA1(this string Str)
        {
            using (SHA1Managed Hasher = new SHA1Managed())
            {
                byte[] bytes = Encoding.ASCII.GetBytes(Str);
                byte[] hash = Hasher.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public static string HashSHA1(this byte[] bytes)
        {
            using (SHA1Managed Hasher = new SHA1Managed())
            {
                byte[] hash = Hasher.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public static string ToStr(this Dictionary<string, string> List)
        {
            List<string> buff = new List<string>();
            foreach (KeyValuePair<string, string> Param in List)
                buff.Add(string.Format("{0}={1}", Param.Key, HttpUtility.UrlEncode(Param.Value)));

            return string.Join("&", buff);
        }
    }
}
