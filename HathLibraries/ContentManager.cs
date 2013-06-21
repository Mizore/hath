using System.IO;
using HathLibraries;
using System;
using System.Text;

public static class Content
{
    public static T Get<T>(string Name)
    {
        object ob = global::HathLibraries.Resources.ResourceManager.GetObject(Name, global::HathLibraries.Resources.Culture);
        object temp = null;

        switch (ob.GetType().ToString())
        {
            case "System.String":
                switch (typeof(T).ToString())
                {
                    case "System.String": temp = ob; break;
                    case "System.Byte[]": temp = Encoding.UTF8.GetBytes((string)ob); break;
                }
                break;

            case "System.Byte[]":
                switch (typeof(T).ToString())
                {
                    case "System.String": temp = Encoding.UTF8.GetString((byte[])ob); break;
                    case "System.Byte[]": temp = ob; break;
                }
                break;
        }

        return (T)temp;
    }
}

