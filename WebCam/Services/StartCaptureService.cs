using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore.Enums;
using FFMpegCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebCam.Settings;
using WebCam.Interfaces;

namespace WebCam.Services;

public class StartCaptureService : BackgroundService
{
    private readonly List<CaptureParameters> _parameters;
    private readonly ICaptureService _captureService;
    private readonly ILogger _logger;

    public StartCaptureService(IOptions<List<CaptureParameters>> aParameters, ICaptureService aCaptureService, ILogger<CaptureService> aLogger)
    {
        _parameters = aParameters?.Value ?? throw new ArgumentNullException(nameof(aParameters));
        _captureService = aCaptureService ?? throw new ArgumentNullException(nameof(aCaptureService));
        _logger = aLogger ?? throw new ArgumentNullException(nameof(aLogger));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();

        foreach (var parameters in _parameters)
        {
            tasks.Add(_captureService.StartCapture(parameters, stoppingToken));
        }

        return Task.WhenAll(tasks);
    }
}
