namespace WebsiteMonitor.Models;

/// <summary>
/// Root configuration object for JSON serialization.
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// List of user-defined server groups.
    /// </summary>
    public List<ServerGroup> Groups { get; set; } = new();

    /// <summary>
    /// List of server-to-group assignments.
    /// </summary>
    public List<ServerGroupAssignment> Assignments { get; set; } = new();

    /// <summary>
    /// List of process names to ignore (blacklist). These won't be shown in the UI.
    /// </summary>
    public List<string> IgnoredProcessNames { get; set; } = new();

    /// <summary>
    /// List of remembered servers that persist even when stopped.
    /// </summary>
    public List<RememberedServer> RememberedServers { get; set; } = new();
}

/// <summary>
/// A server that should persist in the list even when not running.
/// </summary>
public class RememberedServer
{
    /// <summary>
    /// Process name (e.g., "node", "dotnet", "Ripples.Api").
    /// </summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>
    /// Port number the server listens on.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// The command line used to start this process.
    /// </summary>
    public string? CommandLine { get; set; }

    /// <summary>
    /// The working directory where the process was started.
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// Unique key for this server (processName:port).
    /// </summary>
    public string ServerKey => $"{ProcessName.ToLowerInvariant()}:{Port}";
}
