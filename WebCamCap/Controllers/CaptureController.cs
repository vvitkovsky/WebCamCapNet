using System.Runtime;
using Microsoft.AspNetCore.Mvc;
using WebCam.Interfaces;

namespace WebCamCap.Controllers;

[ApiController]
[Route("api/v1/capture")]
public class CaptureController : ControllerBase
{
    private readonly ICaptureService _captureService;
    private readonly ILogger _logger;

    public CaptureController(ICaptureService aCaptureService, ILogger<CaptureController> aLogger)
    {
        _captureService = aCaptureService ?? throw new ArgumentNullException(nameof(aCaptureService));
        _logger = aLogger ?? throw new ArgumentNullException(nameof(aLogger));
    }

    [HttpGet("devices")]
    public async Task<IActionResult> GetDevices()
    {
        return Ok(await _captureService.ListDevices(CancellationToken.None));
    }
}
