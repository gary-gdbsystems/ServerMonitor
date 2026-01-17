using System.IO;
using System.Text.Json;
using WebsiteMonitor.Models;
using WebsiteMonitor.Services.Interfaces;

namespace WebsiteMonitor.Services;

/// <summary>
/// Service for persisting and loading application configuration.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private static readonly string ConfigDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WebsiteMonitor");

    private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private AppConfiguration _config = new();
    private readonly object _lock = new();

    public ConfigurationService()
    {
        Load();
    }

    public IReadOnlyList<ServerGroup> GetGroups()
    {
        lock (_lock)
        {
            return _config.Groups.ToList();
        }
    }

    public void SaveGroup(ServerGroup group)
    {
        lock (_lock)
        {
            var existingIndex = _config.Groups.FindIndex(g => g.Id == group.Id);
            if (existingIndex >= 0)
            {
                _config.Groups[existingIndex] = group;
            }
            else
            {
                _config.Groups.Add(group);
            }
        }
        Save();
    }

    public void DeleteGroup(string groupId)
    {
        lock (_lock)
        {
            _config.Groups.RemoveAll(g => g.Id == groupId);
            _config.Assignments.RemoveAll(a => a.GroupId == groupId);
        }
        Save();
    }

    public ServerGroupAssignment? GetAssignment(string serverKey)
    {
        lock (_lock)
        {
            return _config.Assignments.FirstOrDefault(a =>
                a.ServerKey.Equals(serverKey, StringComparison.OrdinalIgnoreCase));
        }
    }

    public void AssignServerToGroup(string serverKey, string groupId)
    {
        lock (_lock)
        {
            // Remove existing assignment
            _config.Assignments.RemoveAll(a =>
                a.ServerKey.Equals(serverKey, StringComparison.OrdinalIgnoreCase));

            // Add new assignment
            _config.Assignments.Add(new ServerGroupAssignment
            {
                ServerKey = serverKey,
                GroupId = groupId
            });
        }
        Save();
    }

    public void RemoveServerAssignment(string serverKey)
    {
        lock (_lock)
        {
            _config.Assignments.RemoveAll(a =>
                a.ServerKey.Equals(serverKey, StringComparison.OrdinalIgnoreCase));
        }
        Save();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(ConfigDirectory);

            string json;
            lock (_lock)
            {
                json = JsonSerializer.Serialize(_config, JsonOptions);
            }

            File.WriteAllText(ConfigFilePath, json);
        }
        catch
        {
            // Log error in production
        }
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(ConfigFilePath))
            {
                return;
            }

            var json = File.ReadAllText(ConfigFilePath);
            var config = JsonSerializer.Deserialize<AppConfiguration>(json, JsonOptions);

            if (config != null)
            {
                lock (_lock)
                {
                    _config = config;
                    // Ensure default process names exist
                    if (_config.MonitoredProcessNames.Count == 0)
                    {
                        _config.MonitoredProcessNames = new List<string> { "node", "dotnet" };
                    }
                }
            }
        }
        catch
        {
            // Log error in production, start with empty config
            _config = new AppConfiguration();
        }
    }

    public IReadOnlyList<string> GetMonitoredProcessNames()
    {
        lock (_lock)
        {
            return _config.MonitoredProcessNames.ToList();
        }
    }

    public void AddMonitoredProcessName(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName)) return;

        var name = processName.Trim();
        // Remove .exe extension if present
        if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            name = name[..^4];
        }

        lock (_lock)
        {
            if (!_config.MonitoredProcessNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                _config.MonitoredProcessNames.Add(name);
            }
        }
        Save();
    }

    public void RemoveMonitoredProcessName(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName)) return;

        lock (_lock)
        {
            _config.MonitoredProcessNames.RemoveAll(n =>
                n.Equals(processName, StringComparison.OrdinalIgnoreCase));
        }
        Save();
    }

    public IReadOnlyList<RememberedServer> GetRememberedServers()
    {
        lock (_lock)
        {
            return _config.RememberedServers.ToList();
        }
    }

    public void RememberServer(string processName, int port, string? commandLine = null, string? workingDirectory = null)
    {
        var serverKey = $"{processName}:{port}";

        lock (_lock)
        {
            var existing = _config.RememberedServers.FirstOrDefault(
                s => s.ServerKey.Equals(serverKey, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                // Update command line info if we have new data
                if (!string.IsNullOrEmpty(commandLine) && string.IsNullOrEmpty(existing.CommandLine))
                {
                    existing.CommandLine = commandLine;
                    existing.WorkingDirectory = workingDirectory;
                }
                else
                {
                    return; // Already remembered with command line
                }
            }
            else
            {
                _config.RememberedServers.Add(new RememberedServer
                {
                    ProcessName = processName,
                    Port = port,
                    CommandLine = commandLine,
                    WorkingDirectory = workingDirectory
                });
            }
        }
        Save();
    }

    public void ForgetServer(string serverKey)
    {
        lock (_lock)
        {
            _config.RememberedServers.RemoveAll(s =>
                s.ServerKey.Equals(serverKey, StringComparison.OrdinalIgnoreCase));

            // Also remove any group assignment for this server
            _config.Assignments.RemoveAll(a =>
                a.ServerKey.Equals(serverKey, StringComparison.OrdinalIgnoreCase));
        }
        Save();
    }
}
