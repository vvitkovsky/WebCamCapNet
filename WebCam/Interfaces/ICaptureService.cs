using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCam.Settings;

namespace WebCam.Interfaces;

public interface ICaptureService
{
    Task<string> ListDevices(CancellationToken aCancellationToken);

    Task StartCapture(CaptureParameters aParameters, CancellationToken aCancellationToken);    
}
