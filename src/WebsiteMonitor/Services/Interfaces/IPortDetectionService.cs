namespace WebsiteMonitor.Services.Interfaces;

/// <summary>
/// Service for detecting listening TCP ports by process ID.
/// </summary>
public interface IPortDetectionService
{
    /// <summary>
    /// Gets all listening TCP ports grouped by process ID.
    /// </summary>
    /// <returns>Dictionary mapping PID to list of listening ports.</returns>
    Dictionary<int, List<int>> GetListeningPortsByPid();
}
