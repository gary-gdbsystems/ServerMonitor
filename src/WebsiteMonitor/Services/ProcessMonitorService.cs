using System.Diagnostics;
using System.IO;
using System.Management;
using WebsiteMonitor.Models;
using WebsiteMonitor.Services.Interfaces;
using Timer = System.Threading.Timer;

namespace WebsiteMonitor.Services;

/// <summary>
/// Service for monitoring development server processes.
/// </summary>
public class ProcessMonitorService : IProcessMonitorService, IDisposable
{
    private readonly IPortDetectionService _portDetectionService;
    private readonly IConfigurationService _configService;
    private readonly Timer _refreshTimer;
    private readonly object _lock = new();
    private List<ServerProcess> _currentServers = new();
    private bool _isRunning;

    public event EventHandler<ServerListChangedEventArgs>? ServersChanged;

    public IReadOnlyList<ServerProcess> CurrentServers
    {
        get
        {
            lock (_lock)
            {
                return _currentServers.ToList();
            }
        }
    }

    public ProcessMonitorService(IPortDetectionService portDetectionService, IConfigurationService configService)
    {
        _portDetectionService = portDetectionService;
        _configService = configService;
        _refreshTimer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        if (_isRunning) return;
        _isRunning = true;
        _refreshTimer.Change(0, 2000); // Refresh every 2 seconds
    }

    public void Stop()
    {
        _isRunning = false;
        _refreshTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public void Refresh()
    {
        RefreshServers();
    }

    private void OnTimerElapsed(object? state)
    {
        RefreshServers();
    }

    private void RefreshServers()
    {
        try
        {
            var ignoredNames = new HashSet<string>(_configService.GetIgnoredProcessNames(), StringComparer.OrdinalIgnoreCase);
            var portsByPid = _portDetectionService.GetListeningPortsByPid();
            var newServers = new List<ServerProcess>();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    // Skip if process is in the ignore list
                    if (ignoredNames.Contains(process.ProcessName))
                        continue;

                    // Skip processes that don't have listening ports
                    if (!portsByPid.TryGetValue(process.Id, out var ports))
                        continue;

                    string? executablePath = null;
                    DateTime? startTime = null;

                    try
                    {
                        executablePath = process.MainModule?.FileName;
                        startTime = process.StartTime;
                    }
                    catch
                    {
                        // Access denied for some processes - skip them
                        continue;
                    }

                    // Only include processes that have a valid executable path
                    // This filters out system processes we can't access
                    if (string.IsNullOrEmpty(executablePath))
                        continue;

                    // Get command line and working directory
                    var (commandLine, workingDir) = GetProcessCommandLine(process.Id);

                    foreach (var port in ports)
                    {
                        newServers.Add(new ServerProcess
                        {
                            ProcessId = process.Id,
                            ProcessName = process.ProcessName,
                            Port = port,
                            ExecutablePath = executablePath,
                            StartTime = startTime,
                            CommandLine = commandLine,
                            WorkingDirectory = workingDir
                        });
                    }
                }
                catch
                {
                    // Process may have exited
                }
                finally
                {
                    process.Dispose();
                }
            }

            // Sort by port number
            newServers.Sort((a, b) => a.Port.CompareTo(b.Port));

            bool changed;
            lock (_lock)
            {
                changed = !AreServerListsEqual(_currentServers, newServers);
                if (changed)
                {
                    _currentServers = newServers;
                }
            }

            if (changed)
            {
                ServersChanged?.Invoke(this, new ServerListChangedEventArgs(newServers));
            }
        }
        catch
        {
            // Log error in production
        }
    }

    private static bool AreServerListsEqual(List<ServerProcess> a, List<ServerProcess> b)
    {
        if (a.Count != b.Count) return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (!a[i].Equals(b[i])) return false;
        }

        return true;
    }

    private static (string? commandLine, string? workingDirectory) GetProcessCommandLine(int processId)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                $"SELECT CommandLine, ExecutablePath FROM Win32_Process WHERE ProcessId = {processId}");

            foreach (ManagementObject obj in searcher.Get())
            {
                var commandLine = obj["CommandLine"]?.ToString();
                var executablePath = obj["ExecutablePath"]?.ToString();

                // Try to derive working directory from executable path
                string? workingDir = null;
                if (!string.IsNullOrEmpty(executablePath))
                {
                    workingDir = Path.GetDirectoryName(executablePath);
                }

                return (commandLine, workingDir);
            }
        }
        catch
        {
            // WMI query failed
        }

        return (null, null);
    }

    public void Dispose()
    {
        _refreshTimer.Dispose();
    }
}
