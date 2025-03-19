// <copyright file="CaptureController.cs" author="Victor Vitkovskiy">
//     Copyright (C) Victor Vitkovskiy, Espoo Finland
// </copyright>

using System.Runtime;
using Microsoft.AspNetCore.Mvc;
using WebCam.Interfaces;

namespace WebCamCap.Controllers;

[ApiController]
[Route("api/v1/capture")]
public class CaptureController : ControllerBase
{
    private readonly IListDeviceService _listDeviceService;
    private readonly ILogger _logger;

    public CaptureController(IListDeviceService aListDeviceService, ILogger<CaptureController> aLogger)
    {
        _listDeviceService = aListDeviceService ?? throw new ArgumentNullException(nameof(aListDeviceService));
        _logger = aLogger ?? throw new ArgumentNullException(nameof(aLogger));
    }

    [HttpGet("devices")]
    public async Task<IActionResult> GetDevices()
    {
        return Ok(await _listDeviceService.ListDevices(CancellationToken.None));
    }
}