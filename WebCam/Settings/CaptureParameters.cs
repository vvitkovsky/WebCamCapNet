using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCam.Settings;

public class CaptureParameters
{
    public string? VideoDeviceName { get; set; }

    public string? AudioDeviceName { get; set; }

    public string VideoCodec { get; set; } = "libx264";

    public string AudioCodec { get; set; } = "aac";

    public int FrameRate { get; set; } = 1;

    public int Quality { get; set; } = 20;

    public string OutFilePrefix { get; set; } = "cam";

    public string OutputDir { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out");

}
