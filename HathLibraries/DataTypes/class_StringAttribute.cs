using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HathLibraries.DataTypes
{
    public class StringAttribute : Attribute
    {
        public string Type { get; set; }
        public StringAttribute(string Attribute)
        {
            this.Type = Attribute;
        }
    }
}
