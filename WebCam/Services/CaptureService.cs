using System.Net;
using System.Text;
using System.Threading;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebCam.Interfaces;
using WebCam.Settings;

namespace WebCam.Services;

public class CaptureService : ICaptureService
{
    private readonly ILogger _logger;

    public CaptureService(ILogger<CaptureService> aLogger)
    {
        _logger = aLogger ?? throw new ArgumentNullException(nameof(aLogger));
    }

    public async Task<string> ListDevices(CancellationToken aCancellationToken)
    {
        var output = new StringBuilder();
        var action = new Action<string>((x) =>
        {
            if (x.Contains("dshow") && !x.Contains("Alternative name"))
            {
                var namePos = x.IndexOf("] ");

                if (namePos > 0)
                {
                    namePos += 2;
                    x = x[namePos..];
                }

                output.AppendLine(x);
            }
        });

        try
        {
            await FFMpegArguments
               .FromDeviceInput("dummy", args => args
                   .WithCustomArgument("-list_devices true")
                   .ForceFormat("dshow"))
               .OutputToFile("dummy")
               .CancellableThrough(aCancellationToken, 10000)
               .NotifyOnError(action)
               .NotifyOnOutput(action)
               .ProcessAsynchronously();
        }
        catch (OperationCanceledException)
        {
        }

        return output.ToString();
    }

    public async Task StartCapture(CaptureParameters aParameters, CancellationToken aCancellationToken)
    {
        if (aParameters == null)
        {
            throw new ArgumentNullException(nameof(aParameters));
        }

        Directory.CreateDirectory(aParameters.OutputDir);

        await CheckDevicesExist(aParameters, aCancellationToken);

        var deviceName = GetDeviceName(aParameters);
        if (string.IsNullOrEmpty(deviceName))
        {
            _logger.LogError("Device settings are incorrect!");
            return;
        }

        var filePath = Path.Combine(aParameters.OutputDir, $"{aParameters.OutFilePrefix}_{DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.avi")}");
        var deviceOptions = GetDeviceOptions(aParameters);
        var outputOptions = GetOutputOptions(aParameters);

        try
        {
            await FFMpegArguments
                .FromDeviceInput(deviceName, deviceOptions)
                .OutputToFile(filePath, false, outputOptions)
                .CancellableThrough(aCancellationToken, 10000)
                .WithLogLevel(FFMpegLogLevel.Warning)
                .NotifyOnOutput((x) => _logger.LogInformation(x))
                .NotifyOnError((x) => _logger.LogWarning(x))
                .ProcessAsynchronously();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private Action<FFMpegArgumentOptions> GetDeviceOptions(CaptureParameters aParameters)
    {
        return new Action<FFMpegArgumentOptions>((x) =>
        {
            x.ForceFormat("dshow");

            if (!string.IsNullOrEmpty(aParameters.VideoDeviceName))
            {
                if (!string.IsNullOrEmpty(aParameters.VideoResolution))
                {
                    x.WithCustomArgument($"-video_size {aParameters.VideoResolution}");
                }
                x.WithCustomArgument($"-rtbufsize {aParameters.RTBufferSizeMb}M");                
            }
        });
    }

    private Action<FFMpegArgumentOptions> GetOutputOptions(CaptureParameters aParameters)
    {
        return new Action<FFMpegArgumentOptions>((x) =>
        {
            if (!string.IsNullOrEmpty(aParameters.AudioDeviceName))
            {
                x.WithAudioCodec(FFMpeg.GetCodec(aParameters.AudioCodec));
            }

            if (!string.IsNullOrEmpty(aParameters.VideoDeviceName))
            {
                x.WithVideoCodec(FFMpeg.GetCodec(aParameters.VideoCodec));
                x.WithFramerate(aParameters.FrameRate);
                x.WithConstantRateFactor(aParameters.ConstantRateFactor);                
            }

            x.WithFastStart();
        });
    }

    private bool CheckDevice(string aDevices, string? aDevice)
    {
        if (!string.IsNullOrEmpty(aDevice) && !aDevices.Contains(aDevice, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError($"Device {aDevice} is not found!");
            return false;
        }
        return true;
    }

    private async Task CheckDevicesExist(CaptureParameters aParameters, CancellationToken aCancellationToken)
    {
        var devices = await ListDevices(aCancellationToken);

        if (!CheckDevice(devices, aParameters.AudioDeviceName))
        {
            _logger.LogError($"Audio device {aParameters.AudioDeviceName} is not found!");
            aParameters.AudioDeviceName = string.Empty;
        }

        if (!CheckDevice(devices, aParameters.VideoDeviceName))
        {
            _logger.LogError($"Video device {aParameters.VideoDeviceName} is not found!");
            aParameters.VideoDeviceName = string.Empty;
        }
    }
        
    private static string GetDeviceName(CaptureParameters aParameters)
    {
        var deviceName = new StringBuilder();

        if (!string.IsNullOrEmpty(aParameters.VideoDeviceName))
        {
            deviceName.Append($"video=\"{aParameters.VideoDeviceName}\"");
        }

        if (!string.IsNullOrEmpty(aParameters.AudioDeviceName))
        {
            if (deviceName.Length > 0)
            {
                deviceName.Append(":");
            }
            deviceName.Append($"audio=\"{aParameters.AudioDeviceName}\"");
        }
        return deviceName.ToString();
    }
}
