// <copyright file="CaptureService.cs" author="Victor Vitkovskiy">
//     Copyright (C) Victor Vitkovskiy, Espoo Finland
// </copyright>

using System.Text;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;
using WebCam.Interfaces;
using WebCam.Settings;

namespace WebCam.Services;

public class CaptureService : ICaptureService
{
    private readonly IListDeviceService _listDeviceService;
    private readonly ILogger _logger;

    public CaptureService(IListDeviceService aListDeviceService, ILogger<CaptureService> aLogger)
    {
        _listDeviceService = aListDeviceService ?? throw new ArgumentNullException(nameof(aListDeviceService));
        _logger = aLogger ?? throw new ArgumentNullException(nameof(aLogger));
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

        var filePath = Path.Combine(aParameters.OutputDir, $"{aParameters.OutFilePrefix}_%Y-%m-%d_%H-%M-%S.avi");
        var deviceOptions = GetDeviceOptions(aParameters);
        var outputOptions = GetOutputOptions(aParameters);

        while (!aCancellationToken.WaitHandle.WaitOne(5000))
        {
            try
            {
                var processor = FFMpegArguments
                    .FromDeviceInput(deviceName, deviceOptions)
                    .OutputToFile(filePath, false, outputOptions)
                    .CancellableThrough(aCancellationToken, 10000)
                    .WithLogLevel(FFMpegLogLevel.Warning)
                    .NotifyOnOutput((x) => _logger.LogInformation(x))
                    .NotifyOnError((x) => _logger.LogWarning(x));

                _logger.LogInformation("ffmpeg.exe " + processor.Arguments);

                await processor.ProcessAsynchronously();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Capture error!");
            }
        }
    }

    private static Action<FFMpegArgumentOptions> GetDeviceOptions(CaptureParameters aParameters)
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
            }

            if (aParameters.RTBufferSizeMb > 0)
            {
                x.WithCustomArgument($"-rtbufsize {aParameters.RTBufferSizeMb}M");
            }
        });
    }

    private static Action<FFMpegArgumentOptions> GetOutputOptions(CaptureParameters aParameters)
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
                x.WithSpeedPreset(aParameters.SpeedPreset);
                x.WithCustomArgument("-strftime 1");

                if (aParameters.IntraFrameInterval > 0)
                {
                    x.WithCustomArgument($"-g {aParameters.IntraFrameInterval}");
                }

                if (aParameters.SegmentTimeSec > 0)
                {
                    x.WithCustomArgument($"-f segment -segment_format avi -segment_time {aParameters.SegmentTimeSec} -reset_timestamps 1");
                }

                if (aParameters.SegmentMaxFileCount > 0)
                {
                    x.WithCustomArgument($"-segment_wrap {aParameters.SegmentMaxFileCount}");
                }
            }

            if (!string.IsNullOrEmpty(aParameters.CustomParameters))
            {
                x.WithCustomArgument(aParameters.CustomParameters);
            }
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
        var devices = await _listDeviceService.ListDevices(aCancellationToken);

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
