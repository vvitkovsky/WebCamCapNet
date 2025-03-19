// <copyright file="IListDeviceService.cs" author="Victor Vitkovskiy">
//     Copyright (C) Victor Vitkovskiy, Espoo Finland
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCam.Interfaces;

public interface IListDeviceService
{
    Task<string> ListDevices(CancellationToken aCancellationToken);
}