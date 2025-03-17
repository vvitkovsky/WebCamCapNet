using System.Text;
using System.Threading;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebCam.Settings;

namespace WebCam.Services;

public class CaptureService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly CaptureParameters _parameters;

    public CaptureService(IOptions<CaptureParameters> aParameters, ILogger<CaptureService> aLogger)
    {
        _parameters = aParameters?.Value ?? throw new ArgumentNullException(nameof(aParameters));
        _logger = aLogger ?? throw new ArgumentNullException(nameof(aLogger));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory(_parameters.OutputDir);
        var filePath = Path.Combine(_parameters.OutputDir, $"cam_{DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.avi")}");

        var deviceName = new StringBuilder();
        if (!string.IsNullOrEmpty(_parameters.VideoDeviceName))
        {
            deviceName.Append($"video=\"{_parameters.VideoDeviceName}\"");
        }

        if (!string.IsNullOrEmpty(_parameters.AudioDeviceName))
        {
            if (deviceName.Length > 0)
            {
                deviceName.Append(":");
            }

            deviceName.Append($"audio=\"{_parameters.AudioDeviceName}\"");
        }

        return FFMpegArguments
            .FromDeviceInput(deviceName.ToString(), args => args
                .ForceFormat("dshow"))
            .OutputToFile(filePath, false, options => options
                .WithVideoCodec(FFMpeg.GetCodec(_parameters.Codec))
                .WithFramerate(_parameters.FrameRate)
                .WithAudioCodec(AudioCodec.Aac)
                .WithCustomArgument($"-q {_parameters.Quality}")
            .WithFastStart())
            .CancellableThrough(stoppingToken, 1000)
            .WithLogLevel(FFMpegLogLevel.Info)            
            .NotifyOnOutput((x) => _logger.LogInformation(x))
            .NotifyOnError((x) => _logger.LogWarning(x))
            .ProcessAsynchronously();
    }
}
