using WebsiteMonitor.Native;
using WebsiteMonitor.Services.Interfaces;

namespace WebsiteMonitor.Services;

/// <summary>
/// Service for detecting listening TCP ports using Windows API.
/// </summary>
public class PortDetectionService : IPortDetectionService
{
    /// <inheritdoc/>
    public Dictionary<int, List<int>> GetListeningPortsByPid()
    {
        return IpHlpApi.GetListeningPortsByPid();
    }
}
