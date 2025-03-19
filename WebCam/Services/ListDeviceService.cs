// <copyright file="ListDeviceService.cs" author="Victor Vitkovskiy">
//     Copyright (C) Victor Vitkovskiy, Espoo Finland
// </copyright>

using System.Text;
using FFMpegCore;
using WebCam.Interfaces;

namespace WebCam.Services;

public class ListDeviceService : IListDeviceService
{
    private string? _devices;

    public async Task<string> ListDevices(CancellationToken aCancellationToken)
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

        _devices = output.ToString();
        return _devices;
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
}
