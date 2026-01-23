using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WebsiteMonitor.Models;
using WebsiteMonitor.Services.Interfaces;
using Application = System.Windows.Application;

namespace WebsiteMonitor.ViewModels;

/// <summary>
/// Main ViewModel for the popup window.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IProcessMonitorService _monitorService;
    private readonly IProcessTerminationService _terminationService;
    private readonly IConfigurationService _configService;

    [ObservableProperty]
    private ObservableCollection<ServerRowViewModel> _servers = new();

    [ObservableProperty]
    private string _statusText = "0 servers running";

    public MainViewModel(
        IProcessMonitorService monitorService,
        IProcessTerminationService terminationService,
        IConfigurationService configService)
    {
        _monitorService = monitorService;
        _terminationService = terminationService;
        _configService = configService;

        _monitorService.ServersChanged += OnServersChanged;
        _monitorService.Start();

        // Initial load
        UpdateServers(_monitorService.CurrentServers);
    }

    private void OnServersChanged(object? sender, ServerListChangedEventArgs e)
    {
        Application.Current?.Dispatcher.Invoke(() => UpdateServers(e.Servers));
    }

    private void UpdateServers(IReadOnlyList<ServerProcess> runningServers)
    {
        // Create a map of running servers by their key (processName:port)
        var runningByKey = runningServers.ToDictionary(
            s => s.ServerKey,
            StringComparer.OrdinalIgnoreCase);

        // Create a map of existing VMs by their server key
        var existingVmsByKey = Servers.ToDictionary(
            vm => vm.ServerKey,
            StringComparer.OrdinalIgnoreCase);

        // Get all remembered servers
        var rememberedServers = _configService.GetRememberedServers();

        var newVms = new List<ServerRowViewModel>();

        // First, process all running servers
        foreach (var server in runningServers)
        {
            if (existingVmsByKey.TryGetValue(server.ServerKey, out var existingVm))
            {
                // Update existing VM with running state
                existingVm.UpdateRunningState(true, server);
                existingVm.UpdateGroupName();
                newVms.Add(existingVm);
            }
            else
            {
                // Create new VM for running server
                var vm = new ServerRowViewModel(
                    server,
                    _terminationService,
                    _configService,
                    () => _monitorService.Refresh(),
                    RemoveServer);
                vm.UpdateRunningState(true, server);
                newVms.Add(vm);
            }
        }

        // Then, add remembered servers that are not currently running
        foreach (var remembered in rememberedServers)
        {
            // Skip if already added (running)
            if (runningByKey.ContainsKey(remembered.ServerKey))
                continue;

            if (existingVmsByKey.TryGetValue(remembered.ServerKey, out var existingVm))
            {
                // Update existing VM to stopped state
                existingVm.UpdateRunningState(false);
                existingVm.UpdateGroupName();
                newVms.Add(existingVm);
            }
            else
            {
                // Create a placeholder ServerProcess for the stopped server
                var stoppedServer = new ServerProcess
                {
                    ProcessId = 0,
                    ProcessName = remembered.ProcessName,
                    Port = remembered.Port,
                    CommandLine = remembered.CommandLine,
                    WorkingDirectory = remembered.WorkingDirectory
                };
                var vm = new ServerRowViewModel(
                    stoppedServer,
                    _terminationService,
                    _configService,
                    () => _monitorService.Refresh(),
                    RemoveServer);
                vm.UpdateRunningState(false);
                newVms.Add(vm);
            }
        }

        // Sort by group name (ungrouped at end), then by port number
        var sortedVms = newVms
            .OrderBy(vm => string.IsNullOrEmpty(vm.GroupName) ? 1 : 0)  // Grouped first
            .ThenBy(vm => vm.GroupName ?? string.Empty)                  // Then by group name
            .ThenBy(vm => vm.Server.Port)                                // Then by port
            .ToList();

        Servers.Clear();
        foreach (var vm in sortedVms)
        {
            Servers.Add(vm);
        }

        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        var runningCount = Servers.Count(s => s.IsRunning);
        var totalCount = Servers.Count;

        if (totalCount == 0)
        {
            StatusText = "No servers";
        }
        else if (runningCount == totalCount)
        {
            StatusText = runningCount == 1 ? "1 server running" : $"{runningCount} servers running";
        }
        else
        {
            StatusText = $"{runningCount} of {totalCount} running";
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        _monitorService.Refresh();
    }

    [RelayCommand]
    private void Exit()
    {
        Application.Current?.Shutdown();
    }

    [RelayCommand]
    private void CreateNewGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName)) return;

        var group = new ServerGroup { Name = groupName.Trim() };
        _configService.SaveGroup(group);
    }

    public IReadOnlyList<ServerGroup> AvailableGroups => _configService.GetGroups();

    private void RemoveServer(ServerRowViewModel vm)
    {
        Servers.Remove(vm);
        UpdateStatusText();
    }
}
