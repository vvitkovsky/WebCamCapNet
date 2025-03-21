// <copyright file="ListDeviceService.cs" author="Victor Vitkovskiy">
//     Copyright (C) Victor Vitkovskiy, Espoo Finland
// </copyright>

using System.Text;
using FFMpegCore;
using Microsoft.Extensions.Logging;
using WebCam.Interfaces;

namespace WebCam.Services;

public class ListDeviceService : IListDeviceService, IDisposable
{
    private readonly ILogger _logger;
    private SemaphoreSlim _devicesLock = new SemaphoreSlim(1, 1);
    private string? _devices;
    
    public ListDeviceService(ILogger<ListDeviceService> aLogger)
    {
        _logger = aLogger ?? throw new ArgumentNullException(nameof(aLogger));
    }

    public async Task<string> ListDevices(CancellationToken aCancellationToken)
    {
        await _devicesLock.WaitAsync();
        try
        {
            if (!string.IsNullOrEmpty(_devices))
            {
                return _devices;
            }

            var output = new StringBuilder();
            var action = new Action<string>((x) =>
            {
                if (x.Contains("dshow", StringComparison.OrdinalIgnoreCase) &&
                   !x.Contains("Alternative name", StringComparison.OrdinalIgnoreCase))
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
                var processor = FFMpegArguments
                   .FromDeviceInput("dummy", args => args
                       .WithCustomArgument("-list_devices true")
                       .ForceFormat("dshow"))
                   .OutputToFile("dummy")
                   .CancellableThrough(aCancellationToken, 10000)
                   .NotifyOnError(action)
                   .NotifyOnOutput(action);

                _logger.LogInformation("ffmpeg.exe " + processor.Arguments);

                await processor.ProcessAsynchronously();
            }
            catch (OperationCanceledException)
            {
            }

            _devices = output.ToString();
            return _devices;
        }
        finally
        {
            _devicesLock.Release();
        }
    }

    public IList<string> GetCodecs(CodecType aType)
    {
        var result = new List<string>();
        var videoCodecs = FFMpeg.GetCodecs((FFMpegCore.Enums.CodecType)aType);

        foreach (var codec in videoCodecs)
        {
            result.Add($"Codec name: {codec.Name}, encoding: {codec.EncodingSupported}, decoding: {codec.DecodingSupported}");
        }

        return result;
    }

    public string GetCodecByName(string aCodecName)
    {
        if (FFMpeg.TryGetCodec(aCodecName, out var codec))
        {
            var details = new StringBuilder();
            details.AppendLine($"Codec name: {codec.Name}");
            details.AppendLine($"Encoding: {codec.EncodingSupported}");
            details.AppendLine($"Decoding: {codec.DecodingSupported}");
            details.Append($"Description: ");
            details.Append(codec.Description);
            return details.ToString();
        }
        return string.Empty;
    }

    private volatile bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _devicesLock.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
