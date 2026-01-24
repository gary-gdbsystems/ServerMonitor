# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run Commands

```bash
# Build the project
dotnet build

# Run the application
dotnet run --project src/WebsiteMonitor

# Build release version
dotnet build -c Release
```

## Architecture Overview

ServerMonitor is a .NET 8 WPF system tray application that monitors and manages local development servers by detecting processes with listening TCP ports.

### Core Services (DI-registered singletons)

- **ProcessMonitorService**: Polls running processes every 2 seconds, filters by listening ports, emits `ServersChanged` events when the server list changes
- **PortDetectionService**: Wraps `GetExtendedTcpTable` Windows API to get listening TCP ports by PID
- **ProcessTerminationService**: Three-stage graceful shutdown (CloseMainWindow → Ctrl+C via console attach → Kill)
- **ConfigurationService**: Persists groups, assignments, and remembered servers to `%APPDATA%\WebsiteMonitor\config.json`

### Native Windows Interop (`Native/` folder)

- **IpHlpApi.cs**: P/Invoke for `iphlpapi.dll` - uses `MIB_TCPROW_OWNER_PID` structures to map PIDs to listening ports
- **Kernel32.cs**: P/Invoke for console attachment and Ctrl+C signal generation (`AttachConsole`, `GenerateConsoleCtrlEvent`)

### MVVM Pattern

Uses CommunityToolkit.Mvvm with source generators:
- `[ObservableProperty]` for bindable properties
- `[RelayCommand]` for command binding
- `MainViewModel` manages the server list, `ServerRowViewModel` represents individual servers

### Key Data Flow

1. `ProcessMonitorService` timer fires → scans all processes → filters those with listening ports (via `PortDetectionService`)
2. Server list change triggers `ServersChanged` event
3. `MainViewModel.OnServersChanged` marshals to UI thread, merges running servers with remembered (stopped) servers
4. `ServerRowViewModel` handles stop/start/remove actions per server

### System Tray Integration

Uses Hardcodet.NotifyIcon.Wpf. The tray icon is defined in `App.xaml` as a resource, configured in `App.xaml.cs`. Left-click toggles `MainWindow` visibility, which positions itself near the tray icon.
