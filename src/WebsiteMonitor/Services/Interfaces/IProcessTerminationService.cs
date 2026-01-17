namespace WebsiteMonitor.Services.Interfaces;

/// <summary>
/// Service for terminating processes with graceful shutdown support.
/// </summary>
public interface IProcessTerminationService
{
    /// <summary>
    /// Terminates a process, attempting graceful shutdown first.
    /// </summary>
    /// <param name="processId">The ID of the process to terminate.</param>
    /// <param name="gracefulTimeoutMs">Time to wait for graceful shutdown before force killing.</param>
    /// <returns>True if the process was terminated, false if it failed or was already gone.</returns>
    Task<bool> TerminateProcessAsync(int processId, int gracefulTimeoutMs = 5000);
}
