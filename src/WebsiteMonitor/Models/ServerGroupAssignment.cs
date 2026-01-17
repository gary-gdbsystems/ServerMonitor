namespace WebsiteMonitor.Models;

/// <summary>
/// Represents an assignment of a server to a group.
/// </summary>
public class ServerGroupAssignment
{
    /// <summary>
    /// The server key (processName:port).
    /// </summary>
    public string ServerKey { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the assigned group.
    /// </summary>
    public string GroupId { get; init; } = string.Empty;
}
