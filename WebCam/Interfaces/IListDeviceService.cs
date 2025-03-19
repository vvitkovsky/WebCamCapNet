// <copyright file="IListDeviceService.cs" author="Victor Vitkovskiy">
//     Copyright (C) Victor Vitkovskiy, Espoo Finland
// </copyright>

namespace WebCam.Interfaces;

public interface IListDeviceService
{
    Task<string> ListDevices(CancellationToken aCancellationToken);

    IList<string> GetCodecs(CodecType aType);

    string GetCodecByName(string aCodecName);
}