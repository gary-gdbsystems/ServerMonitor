using WebsiteMonitor.Models;

namespace WebsiteMonitor.Services.Interfaces;

/// <summary>
/// Service for monitoring development server processes.
/// </summary>
public interface IProcessMonitorService
{
    /// <summary>
    /// Event fired when the list of servers changes.
    /// </summary>
    event EventHandler<ServerListChangedEventArgs>? ServersChanged;

    /// <summary>
    /// Gets the current list of monitored servers.
    /// </summary>
    IReadOnlyList<ServerProcess> CurrentServers { get; }

    /// <summary>
    /// Starts monitoring for server processes.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops monitoring for server processes.
    /// </summary>
    void Stop();

    /// <summary>
    /// Forces an immediate refresh of the server list.
    /// </summary>
    void Refresh();
}

/// <summary>
/// Event arguments for when the server list changes.
/// </summary>
public class ServerListChangedEventArgs : EventArgs
{
    public IReadOnlyList<ServerProcess> Servers { get; }

    public ServerListChangedEventArgs(IReadOnlyList<ServerProcess> servers)
    {
        Servers = servers;
    }
}
