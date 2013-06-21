using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HathLibraries.DataTypes
{
    public enum ContentType
    {
        Void,

        [StringAttribute("text/plain")]
        TextPlain,
        [StringAttribute("text/html; charset=UTF-8")]
        TextHtml,
        [StringAttribute("text/css")]
        TextCss,
        [StringAttribute("application/javascript")]
        TextJavascript,

        [StringAttribute("application/octet-stream")]
        Binary,

        [StringAttribute("image/x-icon")]
        ImageIcon,
        [StringAttribute("image/png")]
        ImagePNG,
        [StringAttribute("image/jpeg")]
        ImageJPG,
        [StringAttribute("image/gif")]
        ImageGIF
    }
}
