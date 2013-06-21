using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HathLibraries.DataTypes
{
    public enum LogType : byte
    {
        [StringAttribute("  ~Magenta~[ Debug ]~Default~")]
        Debug = 0x1,
        [StringAttribute("   ~Cyan~[ Info ]~Default~")]
        Info = 0x2,
        [StringAttribute("   ~Green~[ HTTP ]~Default~")]
        Http = 0x3,
        [StringAttribute("~Yellow~[ Warning ]~Default~")]
        Warning = 0x4,
        [StringAttribute("  ~Red~[ Error ]~Default~")]
        Error = 0x5,
        [StringAttribute("")]
        Null = 0x6
    }
}
