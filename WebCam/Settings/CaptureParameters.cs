﻿// <copyright file="CaptureParameters.cs" author="Victor Vitkovskiy">
//     Copyright (C) Victor Vitkovskiy, Espoo Finland
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore.Enums;

namespace WebCam.Settings;

public class CaptureParameters
{
    public string? VideoDeviceName { get; set; }

    public string? AudioDeviceName { get; set; }

    public string? VideoResolution { get; set; }

    public string VideoCodec { get; set; } = "libx264";

    public string AudioCodec { get; set; } = "aac";

    public Speed EncodingSpeed { get; set; } = Speed.UltraFast;

    public int FrameRate { get; set; } = 1;
        
    public int ConstantRateFactor { get; set; } = 23;
    
    public string OutFilePrefix { get; set; } = "cam";

    public string OutputDir { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out");

    public int RTBufferSizeMb { get; set; } = 0;

    public int IntraFrameInterval { get; set; } = 0;

    public int SegmentTimeSec { get; set; } = 0;

    public int SegmentMaxFileCount { get; set; } = 0;

    public string? CustomParameters { get; set; }
}
