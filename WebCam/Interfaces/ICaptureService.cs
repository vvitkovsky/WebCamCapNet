// <copyright file="ICaptureService.cs" author="Victor Vitkovskiy">
//     Copyright (C) Victor Vitkovskiy, Espoo Finland
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCam.Settings;

namespace WebCam.Interfaces;

public interface ICaptureService
{    
    Task StartCapture(CaptureParameters aParameters, CancellationToken aCancellationToken);    
}
