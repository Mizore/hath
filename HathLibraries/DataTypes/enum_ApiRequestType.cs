using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HathLibraries.DataTypes
{
    enum ApiRequestType
    {
        [StringAttribute("server_stat")]
        ServerStat,
        [StringAttribute("client_login")]
        ClientLogin,
        [StringAttribute("client_start")]
        ClientStart,
        [StringAttribute("still_alive")]
        StillAlive,
        [StringAttribute("get_blacklist")]
        Backlist,
        [StringAttribute("client_stop")]
        ClientStop,
        [StringAttribute("download_list")]
        DownloadList,
        [StringAttribute("file_register")]
        FileRegister,
        [StringAttribute("file_uncache")]
        FileUncache,
        [StringAttribute("more_files")]
        MoreFiles,
        [StringAttribute("client_suspend")]
        ClientSuspended,
        [StringAttribute("client_resume")]
        ClientResume,
        [StringAttribute("overload")]
        Overload
    }
}
