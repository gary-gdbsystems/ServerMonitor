using WebsiteMonitor.Models;

namespace WebsiteMonitor.Services.Interfaces;

/// <summary>
/// Service for persisting and loading application configuration.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets all defined server groups.
    /// </summary>
    IReadOnlyList<ServerGroup> GetGroups();

    /// <summary>
    /// Adds or updates a server group.
    /// </summary>
    void SaveGroup(ServerGroup group);

    /// <summary>
    /// Deletes a server group.
    /// </summary>
    void DeleteGroup(string groupId);

    /// <summary>
    /// Gets the group assignment for a server key (processName:port).
    /// </summary>
    ServerGroupAssignment? GetAssignment(string serverKey);

    /// <summary>
    /// Assigns a server to a group.
    /// </summary>
    void AssignServerToGroup(string serverKey, string groupId);

    /// <summary>
    /// Removes a server from its group.
    /// </summary>
    void RemoveServerAssignment(string serverKey);

    /// <summary>
    /// Saves all configuration to disk.
    /// </summary>
    void Save();

    /// <summary>
    /// Loads configuration from disk.
    /// </summary>
    void Load();

    /// <summary>
    /// Gets the list of ignored process names (blacklist).
    /// </summary>
    IReadOnlyList<string> GetIgnoredProcessNames();

    /// <summary>
    /// Adds a process name to the ignore list (blacklist).
    /// </summary>
    void AddIgnoredProcessName(string processName);

    /// <summary>
    /// Removes a process name from the ignore list.
    /// </summary>
    void RemoveIgnoredProcessName(string processName);

    /// <summary>
    /// Checks if a process name is in the ignore list.
    /// </summary>
    bool IsProcessIgnored(string processName);

    /// <summary>
    /// Gets all remembered servers that persist even when stopped.
    /// </summary>
    IReadOnlyList<RememberedServer> GetRememberedServers();

    /// <summary>
    /// Remembers a server so it persists in the list even when stopped.
    /// </summary>
    void RememberServer(string processName, int port, string? commandLine = null, string? workingDirectory = null);

    /// <summary>
    /// Forgets a server, removing it from the remembered list.
    /// </summary>
    void ForgetServer(string serverKey);
}
