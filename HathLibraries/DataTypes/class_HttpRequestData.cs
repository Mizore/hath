using HttpMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HathLibraries.DataTypes
{
    public class HttpRequestData : IHttpParserDelegate
    {
        public string Method { get; private set; }
        public string RequestUri { get; private set; }

        public void OnMessageBegin(HttpParser parser)
        {
        }

        public void OnMethod(HttpParser parser, string method)
        {
            this.Method = method;
        }

        public void OnRequestUri(HttpParser parser, string requestUri)
        {
            this.RequestUri = requestUri;
        }

        public void OnPath(HttpParser parser, string path)
        {
        }

        public void OnFragment(HttpParser parser, string fragment)
        {
        }

        public void OnQueryString(HttpParser parser, string queryString)
        {
        }

        public void OnHeaderName(HttpParser parser, string name)
        {
        }

        public void OnHeaderValue(HttpParser parser, string value)
        {
        }

        public void OnHeadersEnd(HttpParser parser)
        {
        }

        public void OnBody(HttpParser parser, ArraySegment<byte> data)
        {
        }

        public void OnMessageEnd(HttpParser parser)
        {
        }
    }
}
