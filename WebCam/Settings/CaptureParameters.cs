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

    public int FrameRate { get; set; } = 1;

    public int Quality { get; set; } = 20;

    public int BufferSize { get; set; } = 1024;

    public string Codec { get; set; } = null!;

    public string OutputDir { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out");
}
