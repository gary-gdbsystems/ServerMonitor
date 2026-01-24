using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebsiteMonitor.Models;
using WebsiteMonitor.Services.Interfaces;
using Clipboard = System.Windows.Clipboard;

namespace WebsiteMonitor.ViewModels;

/// <summary>
/// ViewModel for an individual server row.
/// </summary>
public partial class ServerRowViewModel : ObservableObject
{
    private readonly IProcessTerminationService _terminationService;
    private readonly IConfigurationService _configService;
    private readonly Action _onRefresh;
    private readonly Action<ServerRowViewModel>? _onRemove;

    [ObservableProperty]
    private ServerProcess _server;

    [ObservableProperty]
    private string? _groupName;

    [ObservableProperty]
    private bool _isTerminating;

    [ObservableProperty]
    private bool _isRunning = true;

    [ObservableProperty]
    private bool _isStarting;

    // Stored command line info for restarting
    private string? _commandLine;
    private string? _workingDirectory;

    /// <summary>
    /// Unique key for this server (processName:port).
    /// </summary>
    public string ServerKey => $"{Server.ProcessName.ToLowerInvariant()}:{Server.Port}";

    /// <summary>
    /// Whether this server can be started (has stored command line info).
    /// </summary>
    public bool CanStart => !string.IsNullOrEmpty(_commandLine);

    public ServerRowViewModel(
        ServerProcess server,
        IProcessTerminationService terminationService,
        IConfigurationService configService,
        Action onRefresh,
        Action<ServerRowViewModel>? onRemove = null)
    {
        _server = server;
        _terminationService = terminationService;
        _configService = configService;
        _onRefresh = onRefresh;
        _onRemove = onRemove;

        // Store command line info
        _commandLine = server.CommandLine;
        _workingDirectory = server.WorkingDirectory;

        // Remember this server so it persists when stopped
        _configService.RememberServer(server.ProcessName, server.Port, server.CommandLine, server.WorkingDirectory);

        UpdateGroupName();
    }

    public void UpdateGroupName()
    {
        var assignment = _configService.GetAssignment(Server.ServerKey);
        if (assignment != null)
        {
            var group = _configService.GetGroups().FirstOrDefault(g => g.Id == assignment.GroupId);
            GroupName = group?.Name;
        }
        else
        {
            GroupName = null;
        }
    }

    [RelayCommand]
    private async Task KillProcessAsync()
    {
        if (IsTerminating || !IsRunning) return;

        IsTerminating = true;
        try
        {
            await _terminationService.TerminateProcessAsync(Server.ProcessId);
            // Refresh to update the running state (server will remain in list as stopped)
            _onRefresh();
        }
        finally
        {
            IsTerminating = false;
        }
    }

    [RelayCommand]
    private void Remove()
    {
        // Forget the server and remove it from the list
        _configService.ForgetServer(ServerKey);

        // Directly remove from UI if callback provided
        if (_onRemove != null)
        {
            _onRemove(this);
        }
        else
        {
            _onRefresh();
        }
    }

    [RelayCommand]
    private async Task StartAsync()
    {
        if (IsRunning || IsStarting || string.IsNullOrEmpty(_commandLine)) return;

        IsStarting = true;
        try
        {
            await Task.Run(() =>
            {
                // Parse the command line to extract executable and arguments
                var (executable, arguments) = ParseCommandLine(_commandLine);

                var startInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    WorkingDirectory = _workingDirectory ?? Environment.CurrentDirectory,
                    UseShellExecute = true
                };

                Process.Start(startInfo);
            });

            // Wait a moment for the process to start, then refresh
            await Task.Delay(2000);
            _onRefresh();
        }
        finally
        {
            IsStarting = false;
        }
    }

    private static (string executable, string arguments) ParseCommandLine(string commandLine)
    {
        commandLine = commandLine.Trim();

        // If starts with quote, find the closing quote
        if (commandLine.StartsWith('"'))
        {
            var endQuote = commandLine.IndexOf('"', 1);
            if (endQuote > 0)
            {
                var exe = commandLine.Substring(1, endQuote - 1);
                var args = endQuote + 1 < commandLine.Length
                    ? commandLine.Substring(endQuote + 1).TrimStart()
                    : string.Empty;
                return (exe, args);
            }
        }

        // Otherwise split on first space
        var spaceIndex = commandLine.IndexOf(' ');
        if (spaceIndex > 0)
        {
            return (commandLine.Substring(0, spaceIndex), commandLine.Substring(spaceIndex + 1).TrimStart());
        }

        return (commandLine, string.Empty);
    }

    /// <summary>
    /// Updates the running state based on whether the process is currently active.
    /// </summary>
    public void UpdateRunningState(bool isRunning, ServerProcess? runningServer = null)
    {
        IsRunning = isRunning;
        if (isRunning && runningServer != null)
        {
            // Update with the current process info
            Server = runningServer;

            // Update command line info if available
            if (!string.IsNullOrEmpty(runningServer.CommandLine))
            {
                _commandLine = runningServer.CommandLine;
                _workingDirectory = runningServer.WorkingDirectory;
                OnPropertyChanged(nameof(CanStart));
            }
        }
    }

    [RelayCommand]
    private void CopyPort()
    {
        Clipboard.SetText(Server.Port.ToString());
    }

    [RelayCommand]
    private void OpenInBrowser()
    {
        var url = $"http://localhost:{Server.Port}";
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    [RelayCommand]
    private void AssignToGroup(ServerGroup group)
    {
        _configService.AssignServerToGroup(Server.ServerKey, group.Id);
        UpdateGroupName();
        _onRefresh(); // Refresh to update grouping in UI
    }

    [RelayCommand]
    private void RemoveFromGroup()
    {
        _configService.RemoveServerAssignment(Server.ServerKey);
        UpdateGroupName();
        _onRefresh(); // Refresh to update grouping in UI
    }

    [RelayCommand]
    private void IgnoreProcess()
    {
        // Add this process name to the ignore list
        _configService.AddIgnoredProcessName(Server.ProcessName);
        // Also forget this server and remove from UI
        _configService.ForgetServer(ServerKey);

        if (_onRemove != null)
        {
            _onRemove(this);
        }
        else
        {
            _onRefresh();
        }
    }

    public void CreateAndAssignToGroup(string groupName)
    {
        var group = new ServerGroup { Name = groupName.Trim() };
        _configService.SaveGroup(group);
        _configService.AssignServerToGroup(Server.ServerKey, group.Id);
        UpdateGroupName();
    }

    public IReadOnlyList<ServerGroup> AvailableGroups => _configService.GetGroups();
}
