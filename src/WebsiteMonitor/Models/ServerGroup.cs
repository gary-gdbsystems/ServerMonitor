namespace WebsiteMonitor.Models;

/// <summary>
/// Represents a user-defined group for organizing servers.
/// </summary>
public class ServerGroup
{
    /// <summary>
    /// Unique identifier for the group.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name of the group (e.g., "Frontend", "Backend").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional color for visual distinction (hex format).
    /// </summary>
    public string? Color { get; set; }
}
