using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HathLibraries.DataTypes
{
    public enum HttpStatusCode
    {
        [StringAttribute("200 OK")]
        OK_200,
        [StringAttribute("404 Not Found")]
        NotFount_404
    }
}
