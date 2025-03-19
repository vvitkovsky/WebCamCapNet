// <copyright file="ICaptureService.cs" author="Victor Vitkovskiy">
//     Copyright (C) Victor Vitkovskiy, Espoo Finland
// </copyright>

using WebCam.Settings;

namespace WebCam.Interfaces;

public interface ICaptureService
{    
    Task StartCapture(CaptureParameters aParameters, CancellationToken aCancellationToken);    
}
