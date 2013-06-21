using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HathLibraries.DataTypes;
using System.Net;

namespace HathLibraries
{
    public static class EHApi
    {
        public static EHApiResponse ServerStat()
        {
            return new EHApiResponse(Request(ApiRequestType.ServerStat));
        }

        public static EHApiResponse ClientLogin()
        {
            return new EHApiResponse(Request(ApiRequestType.ClientLogin));
        }

        public static EHApiResponse ClientStart()
        {
            return new EHApiResponse(Request(ApiRequestType.ClientStart));
        }

        public static EHApiResponse StillAlive()
        {
            return new EHApiResponse(Request(ApiRequestType.StillAlive));
        }

        public static EHApiResponse ClientStop()
        {
            return new EHApiResponse(Request(ApiRequestType.ClientStop));
        }

        public static EHApiResponse RegisterPending(EHFile[] Files)
        {
            string add = string.Join(";", (object[])Files);
            return new EHApiResponse(Request(ApiRequestType.FileRegister, add));
        }

        private static string Request(ApiRequestType Type, string Add = "")
        {
            using (var client = new WebClient())
            {
                Stats.Trigger.HttpRequestSent();

                //client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                Dictionary<string, string> Params = new Dictionary<string,string>();
                Params.Add("clientbuild", Configuration.ClientBuild);
                Params.Add("act", Type.GetStringValue());

                if (Type != ApiRequestType.ServerStat)
                {
                    Params.Add("add", Add);
                    Params.Add("cid", Configuration.Account.ClientID);
                    Params.Add("acttime", Configuration.Account.CorrectedTime.ToString());
                    Params.Add("actkey", string.Format("hentai@home-{0}-{1}-{2}-{3}-{4}", Type.GetStringValue(), Add, Configuration.Account.ClientID, Configuration.Account.CorrectedTime, Configuration.Account.ClientKey).HashSHA1());
                }

                byte[] resp = client.DownloadData(string.Format("{0}?{1}", Configuration.ApiHost, Params.ToStr()));
                return Encoding.ASCII.GetString(resp);
            }
        }
    }
}
