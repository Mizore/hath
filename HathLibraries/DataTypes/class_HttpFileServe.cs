using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HathLibraries.DataTypes
{
    public static class HttpFileServe
    {
        public static ResourceFuncData Download(string h, string fileid, string key, string filename)
        {
            EHFile ehf = new EHFile(EHFile.BuildExpectedLocation(fileid));
            if (!ehf.PreFail)
            {
                return new ResourceFuncData(
                    File.ReadAllBytes(ehf.Location),
                    Configuration.ImageContentTypeMap[ehf.Format]);
            }
            else
            {
                return new ResourceFuncData(new byte[] { });
            }
        }

        public static ResourceFuncData Proxy(string p, string com, string file)
        {
            Dictionary<string, string> proxparam = Helpers.ParseAdditional(com);

            string fileid = proxparam["fileid"];
            string token = proxparam["token"];
            string szGid = proxparam["gid"];
            string szPage = proxparam["page"];
            string filename = file;

            EHFile ehf = new EHFile(EHFile.BuildExpectedLocation(fileid));
            if (!ehf.PreFail)
            {
                return new ResourceFuncData(
                    File.ReadAllBytes(ehf.Location),
                    Configuration.ImageContentTypeMap[ehf.Format]);
            }
            else
            {
                EHFile nhf;

                if (DatabaseHandler.FileCache.Download(new EHUri(new string[] { Configuration.Config.RequestServer, fileid, token, szGid, szPage, filename }, EHUriType.Proxy), out nhf))
                {
                    DatabaseHandler.FileCache.RegisterPending(nhf);

                    return new ResourceFuncData(
                        File.ReadAllBytes(nhf.Location),
                        Configuration.ImageContentTypeMap[nhf.Format]);
                }
                else
                    return new ResourceFuncData(new byte[] { });
            }
        }
    }
}
