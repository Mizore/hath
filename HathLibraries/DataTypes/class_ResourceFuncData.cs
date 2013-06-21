using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HathLibraries.DataTypes
{
    public class ResourceFuncData
    {
        public byte[] Data { get; private set; }
        public ContentType ContentType { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }

        public ResourceFuncData(byte[] data, ContentType content = ContentType.Void, Dictionary<string, string> AdditionalHeaders = null)
        {
            this.Data = data;
            this.ContentType = content;
            this.Headers = AdditionalHeaders;
        }
    }
}
