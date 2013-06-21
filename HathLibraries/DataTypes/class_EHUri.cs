using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HathLibraries.DataTypes
{
    public class EHUri
    {
        public string Url { get; private set; }
        public string FileName { get; private set; }
        public EHUriType UriType { get; private set; }

        public EHUri(string[] Peaces, EHUriType Type)
        {
            this.UriType = Type;

            switch(Type)
            {
                case EHUriType.Download:
                    this.FileName = Peaces[1];
                    this.Url = "http://" + Peaces[0] + "/image.php?f=" + Peaces[1] + "&t=" + Peaces[2];
                    break;

                case EHUriType.HathDl:
                    this.FileName = Peaces[1];
                    this.Url = "http://" + Peaces[0] + "/r/" + Peaces[1] + "/" + Peaces[2] + "/" + Peaces[3] + "/" + Peaces[4];
                    break;

                case EHUriType.Proxy:
                    this.FileName = Peaces[1];
                    this.Url = "http://" + Peaces[0] + "/r/" + Peaces[1] + "/" + Peaces[2] + "/" + Peaces[3] + "-" + Peaces[4] + "/" + Peaces[5];
                    break;

                case EHUriType.ProxyTest:
                    this.FileName = Peaces[2];
                    this.Url = "http://" + Peaces[0] + ":" + Peaces[1] + "/h/" + Peaces[2] + "/keystamp=" + Peaces[3] + "/test.jpg";
                    break;
            }
        }
    }
}
