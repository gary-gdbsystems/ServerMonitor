namespace WebsiteMonitor.Models;

/// <summary>
/// Represents a running development server process.
/// </summary>
public class ServerProcess
{
    /// <summary>
    /// The process ID.
    /// </summary>
    public int ProcessId { get; init; }

    /// <summary>
    /// The process name (e.g., "node" or "dotnet").
    /// </summary>
    public string ProcessName { get; init; } = string.Empty;

    /// <summary>
    /// The TCP port the server is listening on.
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// The full path to the process executable, if available.
    /// </summary>
    public string? ExecutablePath { get; init; }

    /// <summary>
    /// When the process was started.
    /// </summary>
    public DateTime? StartTime { get; init; }

    /// <summary>
    /// The command line arguments used to start the process.
    /// </summary>
    public string? CommandLine { get; init; }

    /// <summary>
    /// The working directory where the process was started.
    /// </summary>
    public string? WorkingDirectory { get; init; }

    /// <summary>
    /// Gets the server key used for group assignments (processName:port).
    /// </summary>
    public string ServerKey => $"{ProcessName.ToLowerInvariant()}:{Port}";

    public override bool Equals(object? obj)
    {
        if (obj is ServerProcess other)
        {
            return ProcessId == other.ProcessId && Port == other.Port;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ProcessId, Port);
    }
}
