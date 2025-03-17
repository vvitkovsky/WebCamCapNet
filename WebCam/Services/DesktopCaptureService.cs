using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore.Enums;
using FFMpegCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebCam.Settings;

namespace WebCam.Services;

public class DesktopCaptureService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly CaptureParameters _parameters;

    public DesktopCaptureService(IOptions<CaptureParameters> aParameters, ILogger<CaptureService> aLogger)
    {
        _parameters = aParameters?.Value ?? throw new ArgumentNullException(nameof(aParameters));
        _logger = aLogger ?? throw new ArgumentNullException(nameof(aLogger));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {     
        Directory.CreateDirectory(_parameters.OutputDir);
        var filePath = Path.Combine(_parameters.OutputDir, $"desktop_{DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.avi")}");

        return FFMpegArguments
            .FromDeviceInput("video=\"UScreenCapture\"", args => args
                .ForceFormat("dshow"))
            .OutputToFile(filePath, false, options => options
                .WithVideoCodec(FFMpeg.GetCodec(_parameters.Codec))
                .WithFramerate(_parameters.FrameRate)
                .WithCustomArgument($"-q {_parameters.Quality}")
            .WithFastStart())
            .CancellableThrough(stoppingToken, 1000)
            .NotifyOnOutput((x) => _logger.LogInformation(x))
            .NotifyOnError((x) => _logger.LogWarning(x))
            .WithLogLevel(FFMpegLogLevel.Info)
            .ProcessAsynchronously();
    }
}
