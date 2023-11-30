﻿using System.Collections.Generic;
using HLab.Sys.Windows.API;

namespace HLab.Sys.Windows.Monitors;

public interface ISystemMonitorsService
{
    public DisplayDevice Root {get; set; }

    DesktopWallpaperPosition WallpaperPosition { get; }
}